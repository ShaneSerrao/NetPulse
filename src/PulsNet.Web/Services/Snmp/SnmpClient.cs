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
    }
}