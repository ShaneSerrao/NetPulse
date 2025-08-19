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

                    var delay = TimeSpan.FromSeconds(Math.Max(1, settings.GlobalPollIntervalSeconds));
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
                var sample = new TrafficSample
                {
                    DeviceId = device.Id,
                    Timestamp = DateTimeOffset.UtcNow,
                    DownloadMbps = 0,
                    UploadMbps = 0,
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