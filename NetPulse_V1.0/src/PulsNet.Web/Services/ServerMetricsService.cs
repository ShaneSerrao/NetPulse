using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace PulsNet.Web.Services
{
    public class ServerMetrics
    {
        public CpuMetrics Cpu { get; set; } = new CpuMetrics();
        public MemoryMetrics Memory { get; set; } = new MemoryMetrics();
        public NetworkMetrics Network { get; set; } = new NetworkMetrics();
        public MotherboardMetrics Motherboard { get; set; } = new MotherboardMetrics();
        public DateTime TimestampUtc { get; set; }
    }

    public class CpuMetrics
    {
        public int Cores { get; set; }
        public string Model { get; set; } = string.Empty;
        public double MHz { get; set; }
        public double UtilizationPercent { get; set; }
        public double? TemperatureC { get; set; }
    }

    public class MemoryMetrics
    {
        public double TotalMB { get; set; }
        public double UsedMB { get; set; }
        public double UsedPercent { get; set; }
        public double? TemperatureC { get; set; }
    }

    public class NetworkMetrics
    {
        public double RxMbps { get; set; }
        public double TxMbps { get; set; }
    }

    public class MotherboardMetrics
    {
        public double? TemperatureC { get; set; }
    }

    public class ServerMetricsService : BackgroundService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<ServerMetricsService> _logger;

        private (ulong user, ulong nice, ulong system, ulong idle, ulong iowait, ulong irq, ulong softirq, ulong steal, DateTime t) _prevCpu;
        private Dictionary<string,(ulong rx, ulong tx)> _prevNet = new();
        private DateTime _prevNetTime;

        public ServerMetricsService(IMemoryCache cache, ILogger<ServerMetricsService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public ServerMetrics GetCurrent()
        {
            return _cache.TryGetValue<ServerMetrics>("server:metrics", out var m) && m != null ? m : new ServerMetrics { TimestampUtc = DateTime.UtcNow };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var metrics = new ServerMetrics
                    {
                        Cpu = ReadCpuMetrics(),
                        Memory = ReadMemoryMetrics(),
                        Network = ReadNetworkMetrics(),
                        Motherboard = new MotherboardMetrics { TemperatureC = ReadAnyTemperatureC(labelHint: "board") }
                    };
                    metrics.TimestampUtc = DateTime.UtcNow;
                    _cache.Set("server:metrics", metrics, TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Server metrics collection failure");
                }
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        private CpuMetrics ReadCpuMetrics()
        {
            var cpu = new CpuMetrics();
            try
            {
                if (File.Exists("/proc/cpuinfo"))
                {
                    var text = File.ReadAllText("/proc/cpuinfo");
                    var cores = text.Split('\n').Count(l => l.StartsWith("processor"));
                    cpu.Cores = cores > 0 ? cores : Environment.ProcessorCount;
                    var modelLine = text.Split('\n').FirstOrDefault(l => l.StartsWith("model name"));
                    if (modelLine != null) cpu.Model = modelLine.Split(':').Last().Trim();
                    var mhzLine = text.Split('\n').FirstOrDefault(l => l.StartsWith("cpu MHz"));
                    if (mhzLine != null && double.TryParse(mhzLine.Split(':').Last().Trim(), System.Globalization.CultureInfo.InvariantCulture, out var mhz)) cpu.MHz = mhz;
                }

                var now = DateTime.UtcNow;
                if (File.Exists("/proc/stat"))
                {
                    var first = File.ReadLines("/proc/stat").FirstOrDefault();
                    if (!string.IsNullOrEmpty(first) && first.StartsWith("cpu "))
                    {
                        var parts = Regex.Split(first, "\\s+").Where(p => p.Length > 0).ToArray();
                        ulong user = ulong.Parse(parts[1]);
                        ulong nice = ulong.Parse(parts[2]);
                        ulong system = ulong.Parse(parts[3]);
                        ulong idle = ulong.Parse(parts[4]);
                        ulong iowait = parts.Length > 5 ? ulong.Parse(parts[5]) : 0;
                        ulong irq = parts.Length > 6 ? ulong.Parse(parts[6]) : 0;
                        ulong softirq = parts.Length > 7 ? ulong.Parse(parts[7]) : 0;
                        ulong steal = parts.Length > 8 ? ulong.Parse(parts[8]) : 0;

                        if (_prevCpu.t != default)
                        {
                            var prevIdle = _prevCpu.idle + _prevCpu.iowait;
                            var idleNow = idle + iowait;
                            var prevNonIdle = _prevCpu.user + _prevCpu.nice + _prevCpu.system + _prevCpu.irq + _prevCpu.softirq + _prevCpu.steal;
                            var nonIdleNow = user + nice + system + irq + softirq + steal;
                            var prevTotal = prevIdle + prevNonIdle;
                            var totalNow = idleNow + nonIdleNow;

                            var totald = (double)(totalNow - prevTotal);
                            var idled = (double)(idleNow - prevIdle);
                            if (totald > 0)
                            {
                                cpu.UtilizationPercent = Math.Max(0, Math.Min(100, (1.0 - (idled / totald)) * 100.0));
                            }
                        }
                        _prevCpu = (user, nice, system, idle, iowait, irq, softirq, steal, now);
                    }
                }

                cpu.TemperatureC = ReadAnyTemperatureC(labelHint: "cpu");
            }
            catch { }
            return cpu;
        }

        private MemoryMetrics ReadMemoryMetrics()
        {
            var mem = new MemoryMetrics();
            try
            {
                if (File.Exists("/proc/meminfo"))
                {
                    var dict = File.ReadAllLines("/proc/meminfo")
                        .Select(line => line.Split(':'))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
                    double kb(string key) => dict.TryGetValue(key, out var v) ? double.Parse(Regex.Match(v, "\\d+").Value) : 0;
                    var totalKb = kb("MemTotal");
                    var availableKb = kb("MemAvailable");
                    var usedKb = totalKb - availableKb;
                    mem.TotalMB = Math.Round(totalKb / 1024.0, 1);
                    mem.UsedMB = Math.Round(usedKb / 1024.0, 1);
                    mem.UsedPercent = totalKb > 0 ? Math.Round((usedKb / totalKb) * 100.0, 1) : 0;
                }
                mem.TemperatureC = ReadAnyTemperatureC(labelHint: "mem");
            }
            catch { }
            return mem;
        }

        private NetworkMetrics ReadNetworkMetrics()
        {
            var nm = new NetworkMetrics();
            try
            {
                if (!File.Exists("/proc/net/dev")) return nm;
                var now = DateTime.UtcNow;
                var lines = File.ReadAllLines("/proc/net/dev").Skip(2);
                var curr = new Dictionary<string,(ulong rx, ulong tx)>();
                foreach (var line in lines)
                {
                    var parts = line.Split(':'); if (parts.Length != 2) continue;
                    var iface = parts[0].Trim();
                    if (iface == "lo") continue;
                    var vals = Regex.Split(parts[1].Trim(), "\\s+");
                    if (vals.Length < 16) continue;
                    ulong rx = ulong.Parse(vals[0]);
                    ulong tx = ulong.Parse(vals[8]);
                    curr[iface] = (rx, tx);
                }
                if (_prevNetTime != default && _prevNet.Count > 0)
                {
                    var seconds = (now - _prevNetTime).TotalSeconds;
                    if (seconds > 0)
                    {
                        double rxBps = 0, txBps = 0;
                        foreach (var kv in curr)
                        {
                            if (_prevNet.TryGetValue(kv.Key, out var prev))
                            {
                                var dr = (double)(kv.Value.rx - prev.rx); if (dr < 0) dr = 0;
                                var dt = (double)(kv.Value.tx - prev.tx); if (dt < 0) dt = 0;
                                rxBps += dr / seconds; txBps += dt / seconds;
                            }
                        }
                        nm.RxMbps = Math.Round((rxBps * 8.0) / 1_000_000.0, 2);
                        nm.TxMbps = Math.Round((txBps * 8.0) / 1_000_000.0, 2);
                    }
                }
                _prevNet = curr; _prevNetTime = now;
            }
            catch { }
            return nm;
        }

        private double? ReadAnyTemperatureC(string? labelHint)
        {
            try
            {
                foreach (var hwmon in Directory.EnumerateDirectories("/sys/class/hwmon"))
                {
                    var labels = Directory.EnumerateFiles(hwmon, "temp*_label", SearchOption.TopDirectoryOnly);
                    foreach (var labelFile in labels)
                    {
                        var label = File.ReadAllText(labelFile).Trim().ToLowerInvariant();
                        if (labelHint != null && !label.Contains(labelHint)) continue;
                        var inputFile = labelFile.Replace("_label", "_input");
                        if (File.Exists(inputFile))
                        {
                            var valStr = File.ReadAllText(inputFile).Trim();
                            if (double.TryParse(valStr, out var milliC)) return Math.Round(milliC / 1000.0, 1);
                        }
                    }
                    var any = Directory.EnumerateFiles(hwmon, "temp*_input", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    if (any != null)
                    {
                        var valStr = File.ReadAllText(any).Trim();
                        if (double.TryParse(valStr, out var milliC)) return Math.Round(milliC / 1000.0, 1);
                    }
                }
            }
            catch { }
            return null;
        }
    }
}