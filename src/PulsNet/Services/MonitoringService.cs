using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Caching.Memory;
using PulsNet.Data;

namespace PulsNet.Services
{
    public sealed class MonitoringService
    {
        private readonly IMemoryCache _cache;
        private readonly ConfigService _config;
        private readonly Db _db;

        public MonitoringService(IMemoryCache cache, ConfigService config, Db db)
        {
            _cache = cache;
            _config = config;
            _db = db;
        }

        public async Task<LiveStats> GetLiveStatsAsync(DeviceRecord device, CancellationToken ct)
        {
            var cacheKey = $"live:{device.Id}";
            if (_cache.TryGetValue(cacheKey, out LiveStats? cached) && cached != null)
            {
                return cached;
            }

            var ping = await PingHostAsync(device.IpAddress);
            var (downMbps, upMbps) = await GetTrafficMbpsAsync(device, ct);
            var usagePercent = device.MaxLinkMbps > 0 ? (int)Math.Round((Math.Max(downMbps, upMbps) / device.MaxLinkMbps) * 100) : 0;
            var stats = new LiveStats
            {
                DeviceId = device.Id,
                LatencyMs = ping.latencyMs,
                Online = ping.online,
                DownloadMbps = downMbps,
                UploadMbps = upMbps,
                LinkUsagePercent = Math.Clamp(usagePercent, 0, 100)
            };

            _cache.Set(cacheKey, stats, TimeSpan.FromSeconds(_config.Config.Polling.CacheSeconds));
            return stats;
        }

        private async Task<(double latencyMs, bool online)> PingHostAsync(string ip)
        {
            try
            {
                using var p = new Ping();
                var reply = await p.SendPingAsync(ip, 2000);
                if (reply.Status == IPStatus.Success)
                {
                    return (reply.RoundtripTime, true);
                }
                return (0, false);
            }
            catch
            {
                return (0, false);
            }
        }

        private async Task<(double down, double up)> GetTrafficMbpsAsync(DeviceRecord device, CancellationToken ct)
        {
            // Use net-snmp tools to query Mikrotik OIDs. We'll sample twice to compute rate.
            var (in1, out1) = await GetIfHCInOutOctetsAsync(device, ct);
            await Task.Delay(1000, ct);
            var (in2, out2) = await GetIfHCInOutOctetsAsync(device, ct);
            var deltaInBits = (in2 - in1) * 8.0;
            var deltaOutBits = (out2 - out1) * 8.0;
            var mbpsDown = deltaInBits / 1_000_000.0;
            var mbpsUp = deltaOutBits / 1_000_000.0;
            return (Math.Max(0, mbpsDown), Math.Max(0, mbpsUp));
        }

        private async Task<(ulong inOctets, ulong outOctets)> GetIfHCInOutOctetsAsync(DeviceRecord device, CancellationToken ct)
        {
            // Target default interface 1. For real use, store interface index with device; here we use 1.
            const string ifHCIn = "1.3.6.1.2.1.31.1.1.1.6.1";  // ifHCInOctets.1
            const string ifHCOut = "1.3.6.1.2.1.31.1.1.1.10.1"; // ifHCOutOctets.1
            var inStr = await SnmpGetAsync(device.IpAddress, device.SnmpCommunity, ifHCIn, ct);
            var outStr = await SnmpGetAsync(device.IpAddress, device.SnmpCommunity, ifHCOut, ct);
            return (ParseCounter64(inStr), ParseCounter64(outStr));
        }

        private static ulong ParseCounter64(string output)
        {
            // Expected: iso.3.6.1... = Counter64: 123456
            var parts = output.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && ulong.TryParse(parts[1], out var val))
            {
                return val;
            }
            var digits = new string(output.Where(char.IsDigit).ToArray());
            return ulong.TryParse(digits, out var parsed) ? parsed : 0UL;
        }

        public static async Task<string> SnmpGetAsync(string ip, string community, string oid, CancellationToken ct)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/snmpget",
                ArgumentList = { "-v", "2c", "-c", community, "-O", "qv", ip, oid },
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            try
            {
                using var proc = Process.Start(startInfo);
                if (proc == null) return string.Empty;
                var output = await proc.StandardOutput.ReadToEndAsync();
                await proc.WaitForExitAsync(ct);
                return output.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public sealed class LiveStats
    {
        public int DeviceId { get; set; }
        public bool Online { get; set; }
        public double LatencyMs { get; set; }
        public double DownloadMbps { get; set; }
        public double UploadMbps { get; set; }
        public int LinkUsagePercent { get; set; }
    }
}

