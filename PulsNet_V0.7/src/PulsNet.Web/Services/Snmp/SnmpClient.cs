using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PulsNet.Web.Services.Snmp
{
    public class SnmpClient
    {
        private static readonly Regex SafeIpRegex = new Regex("^[a-zA-Z0-9\\-\\.:]+$", RegexOptions.Compiled);
        private static readonly Regex SafeCommunityRegex = new Regex("^[\u0020-\u007E]{1,128}$", RegexOptions.Compiled);

        public async Task<string?> GetAsync(string ipAddress, string community, string oid, int port = 161, int timeoutMs = 1500)
        {
            if (!SafeIpRegex.IsMatch(ipAddress)) return null;
            if (!SafeCommunityRegex.IsMatch(community)) return null;
            if (string.IsNullOrWhiteSpace(oid)) return null;

            var startInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/snmpget",
                ArgumentList = { "-v2c", "-c", community, $"{ipAddress}:{port}", oid, "-t", (timeoutMs/1000.0).ToString("0.0") },
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            try
            {
                using var proc = Process.Start(startInfo);
                if (proc == null) return null;
                var output = await proc.StandardOutput.ReadToEndAsync();
                await proc.WaitForExitAsync();
                if (proc.ExitCode != 0) return null;
                return output;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Dictionary<string,long>> BulkWalkCounter64Async(string ipAddress, string community, string baseOid, int port = 161, int timeoutMs = 1500)
        {
            var result = new Dictionary<string,long>();
            if (!SafeIpRegex.IsMatch(ipAddress) || !SafeCommunityRegex.IsMatch(community)) return result;
            var startInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/snmpbulkwalk",
                ArgumentList = { "-v2c", "-c", community, $"{ipAddress}:{port}", baseOid, "-t", (timeoutMs/1000.0).ToString("0.0") },
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            try
            {
                using var proc = Process.Start(startInfo);
                if (proc == null) return result;
                string line;
                while ((line = await proc.StandardOutput.ReadLineAsync() ?? string.Empty) != string.Empty)
                {
                    // format: IF-MIB::ifHCInOctets.1 = Counter64: 123456
                    var idx = line.IndexOf("Counter64:");
                    if (idx > 0)
                    {
                        var oid = line.Substring(0, idx).Trim();
                        var valStr = line.Substring(idx + 10).Trim();
                        if (long.TryParse(valStr, out var val)) result[oid] = val;
                    }
                }
                await proc.WaitForExitAsync();
                return result;
            }
            catch
            {
                return result;
            }
        }

        public static (double inMbps, double outMbps) ComputeMbpsFromCounters((long in1,long out1,DateTime t1) prev, (long in2,long out2,DateTime t2) curr)
        {
            var seconds = (curr.t2 - prev.t1).TotalSeconds;
            if (seconds <= 0) return (0,0);
            var inDelta = curr.in2 - prev.in1; if (inDelta < 0) inDelta = 0; // rollover guard
            var outDelta = curr.out2 - prev.out1; if (outDelta < 0) outDelta = 0;
            var inBps = inDelta / seconds; var outBps = outDelta / seconds; // octets per sec
            var inMbps = (inBps * 8.0) / 1_000_000.0; var outMbps = (outBps * 8.0) / 1_000_000.0;
            return (inMbps, outMbps);
        }
    }
}