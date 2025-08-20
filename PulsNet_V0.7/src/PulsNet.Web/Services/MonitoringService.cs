using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PulsNet.Web.Data;
using PulsNet.Web.Models;
using PulsNet.Web.Services.Snmp;
using System.Diagnostics;

namespace PulsNet.Web.Services
{
    public class MonitoringService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MonitoringService> _logger;
        private const string InOidBase = "IF-MIB::ifHCInOctets";
        private const string OutOidBase = "IF-MIB::ifHCOutOctets";

        public MonitoringService(IServiceProvider serviceProvider, IMemoryCache cache, ILogger<MonitoringService> logger)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var snmp = scope.ServiceProvider.GetRequiredService<SnmpClient>();

                    var settings = await db.AppSettings.AsNoTracking().FirstOrDefaultAsync(stoppingToken) ?? new AppSettings();
                    var devices = await db.Devices.AsNoTracking().ToListAsync(stoppingToken);

                    var tasks = devices.Select(d => PollDeviceAsync(db, snmp, d, stoppingToken));
                    await Task.WhenAll(tasks);

                    var delay = TimeSpan.FromSeconds(Math.Max(2, settings.GlobalPollIntervalSeconds));
                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Monitoring loop failure");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task PollDeviceAsync(AppDbContext db, SnmpClient snmp, Device device, CancellationToken ct)
        {
            try
            {
                var latencyMs = await MeasureLatencyAsync(device.IpAddress, ct);

                var inMap = await snmp.BulkWalkCounter64Async(device.IpAddress, device.SnmpCommunity, InOidBase, device.SnmpPort);
                var outMap = await snmp.BulkWalkCounter64Async(device.IpAddress, device.SnmpCommunity, OutOidBase, device.SnmpPort);

                // aggregate Mbps across all interfaces
                var now = DateTime.UtcNow;
                var prev = _cache.Get<(Dictionary<string,long> inMap, Dictionary<string,long> outMap, DateTime t)>("counters:"+device.Id);
                double totalInMbps = 0, totalOutMbps = 0;
                if (prev.inMap != null && prev.outMap != null && prev.t != default)
                {
                    var seconds = (now - prev.t).TotalSeconds;
                    if (seconds > 0)
                    {
                        foreach (var kv in inMap)
                        {
                            if (prev.inMap.TryGetValue(kv.Key, out var prevIn))
                            {
                                var inDelta = kv.Value - prevIn; if (inDelta < 0) inDelta = 0;
                                totalInMbps += (inDelta / seconds) * 8.0 / 1_000_000.0;
                            }
                        }
                        foreach (var kv in outMap)
                        {
                            if (prev.outMap.TryGetValue(kv.Key, out var prevOut))
                            {
                                var outDelta = kv.Value - prevOut; if (outDelta < 0) outDelta = 0;
                                totalOutMbps += (outDelta / seconds) * 8.0 / 1_000_000.0;
                            }
                        }
                    }
                }
                _cache.Set("counters:"+device.Id, (inMap, outMap, now), TimeSpan.FromMinutes(10));

                var sample = new TrafficSample
                {
                    DeviceId = device.Id,
                    Timestamp = DateTimeOffset.UtcNow,
                    DownloadMbps = totalInMbps,
                    UploadMbps = totalOutMbps,
                    LatencyMs = latencyMs,
                    IsOnline = latencyMs >= 0
                };
                await db.TrafficSamples.AddAsync(sample, ct);
                await db.SaveChangesAsync(ct);
                _cache.Set($"device:{device.Id}:lastSample", sample, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private static async Task<int> MeasureLatencyAsync(string ip, CancellationToken ct)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/ping",
                    ArgumentList = { "-c", "1", "-W", "1", ip },
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using var p = System.Diagnostics.Process.Start(psi);
                if (p == null) return -1;
                var output = await p.StandardOutput.ReadToEndAsync();
                await p.WaitForExitAsync(ct);
                if (p.ExitCode != 0) return -1;
                var idx = output.IndexOf("time=");
                if (idx >= 0)
                {
                    var sub = output[(idx + 5)..];
                    var end = sub.IndexOf(" ");
                    if (end > 0)
                    {
                        var valStr = sub[..end].Replace("ms", "").Trim();
                        if (double.TryParse(valStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var ms))
                        {
                            return (int)Math.Round(ms);
                        }
                    }
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }
    }
}