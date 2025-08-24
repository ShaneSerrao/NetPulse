using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Caching.Memory;
using PulsNet.Data;

namespace PulsNet.Services {
  public sealed class MonitoringService {
    private readonly IMemoryCache _cache;
    private readonly ConfigService _cfg;
    private readonly Db _db;

    public MonitoringService(IMemoryCache cache, ConfigService cfg, Db db) {
      _cache = cache;
      _cfg = cfg;
      _db = db;
    }

    public async Task<Live> LiveStats(
      int deviceId,
      string ip,
      string community,
      int maxLinkMbps,
      int? ifIndex = null,
      int? capDownMbps = null,
      bool capDownEnabled = false,
      int? capUpMbps = null,
      bool capUpEnabled = false,
      CancellationToken ct = default
    ) {
      var key = $"live:{deviceId}";
      if (_cache.TryGetValue(key, out Live? c) && c != null) return c;

      var ping = await Ping(ip);
      var idx = ifIndex ?? 1;
      var (down, up) = await SampleMbps(ip, community, idx, ct);

      // Determine link capacity for usage. Prefer explicit caps per direction; else try ifHighSpeed; else fallback to maxLinkMbps
      var (ifSpeedDown, ifSpeedUp) = await GetIfHighSpeedMbps(ip, community, idx, ct);
      var capD = capDownEnabled && capDownMbps.HasValue && capDownMbps.Value > 0 ? capDownMbps.Value : (ifSpeedDown ?? maxLinkMbps);
      var capU = capUpEnabled && capUpMbps.HasValue && capUpMbps.Value > 0 ? capUpMbps.Value : (ifSpeedUp ?? maxLinkMbps);
      var usage = 0;
      if (capD > 0 || capU > 0) {
        var denom = Math.Max(capD, capU);
        usage = denom > 0 ? (int)Math.Round(Math.Max(down, up) / denom * 100) : 0;
      }

      var res = new Live {
        Online = ping.online,
        LatencyMs = ping.latency,
        DownloadMbps = down,
        UploadMbps = up,
        LinkUsagePercent = Math.Clamp(usage, 0, 100)
      };

      _cache.Set(key, res, TimeSpan.FromSeconds(_cfg.Config.Polling.CacheSeconds));
      return res;
    }

    private static async Task<(bool online, double latency)> Ping(string ip) {
      try {
        using var p = new Ping();
        var r = await p.SendPingAsync(ip, 2000);
        return (r.Status == IPStatus.Success, r.Status == IPStatus.Success ? r.RoundtripTime : 0);
      } catch {
        return (false, 0);
      }
    }

    private async Task<(double down, double up)> SampleMbps(string ip, string comm, int ifIndex, CancellationToken ct) {
      // ifHCInOctets / ifHCOutOctets (64-bit) for accuracy on high-speed links
      var oIn = $"1.3.6.1.2.1.31.1.1.1.6.{ifIndex}";   // ifHCInOctets
      var oOut = $"1.3.6.1.2.1.31.1.1.1.10.{ifIndex}"; // ifHCOutOctets

      var in1 = await Snmp(ip, comm, oIn, ct);
      var out1 = await Snmp(ip, comm, oOut, ct);
      await Task.Delay(1000, ct);
      var in2 = await Snmp(ip, comm, oIn, ct);
      var out2 = await Snmp(ip, comm, oOut, ct);

      ulong di = Delta(in1, in2);
      ulong du = Delta(out1, out2);
      var down = di * 8.0 / 1_000_000.0;
      var up = du * 8.0 / 1_000_000.0;
      return (Math.Max(0, down), Math.Max(0, up));
    }

    private async Task<(int? downMbps, int? upMbps)> GetIfHighSpeedMbps(string ip, string comm, int ifIndex, CancellationToken ct) {
      try {
        // ifHighSpeed returns in Mbps when supported
        var oSpeed = $"1.3.6.1.2.1.31.1.1.1.15.{ifIndex}"; // ifHighSpeed
        var s = await SnmpRawString(ip, comm, oSpeed, ct);
        if (int.TryParse(new string(s.Where(char.IsDigit).ToArray()), out var mbps) && mbps > 0) {
          return (mbps, mbps);
        }
      } catch {}
      return (null, null);
    }

    private static async Task<string> SnmpRawString(string ip, string comm, string oid, CancellationToken ct) {
      var psi = new ProcessStartInfo { FileName = "/usr/bin/snmpget", RedirectStandardOutput = true, RedirectStandardError = true };
      psi.ArgumentList.Add("-v"); psi.ArgumentList.Add("2c");
      psi.ArgumentList.Add("-c"); psi.ArgumentList.Add(comm);
      psi.ArgumentList.Add("-O"); psi.ArgumentList.Add("qv");
      psi.ArgumentList.Add(ip); psi.ArgumentList.Add(oid);
      using var p = Process.Start(psi)!;
      var o = await p.StandardOutput.ReadToEndAsync();
      await p.WaitForExitAsync(ct);
      return o.Trim();
    }

    private static async Task<ulong> Snmp(string ip, string comm, string oid, CancellationToken ct) {
      try {
        var psi = new ProcessStartInfo { FileName = "/usr/bin/snmpget", RedirectStandardOutput = true, RedirectStandardError = true };
        psi.ArgumentList.Add("-v");
        psi.ArgumentList.Add("2c");
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add(comm);
        psi.ArgumentList.Add("-O");
        psi.ArgumentList.Add("qv");
        psi.ArgumentList.Add(ip);
        psi.ArgumentList.Add(oid);

        using var p = Process.Start(psi)!;
        var o = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync(ct);

        var digits = new string(o.Where(char.IsDigit).ToArray());
        return ulong.TryParse(digits, out var v) ? v : 0UL;
      } catch {
        return 0UL;
      }
    }

    private static ulong Delta(ulong a, ulong b) => b >= a ? b - a : (ulong.MaxValue - a + b);

    public sealed class Live {
      public bool Online { get; set; }
      public double LatencyMs { get; set; }
      public double DownloadMbps { get; set; }
      public double UploadMbps { get; set; }
      public int LinkUsagePercent { get; set; }
    }
  }
}
