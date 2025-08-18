using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PulsNet.Data;

namespace PulsNet.Services
{
    public sealed class PollingBackgroundService : BackgroundService
    {
        private readonly ILogger<PollingBackgroundService> _logger;
        private readonly DeviceService _devices;
        private readonly MonitoringService _monitoring;
        private readonly SettingsService _settings;
        private readonly EmailService _email;
        private readonly Db _db;

        public PollingBackgroundService(ILogger<PollingBackgroundService> logger, DeviceService devices, MonitoringService monitoring, SettingsService settings, EmailService email, Db db)
        {
            _logger = logger;
            _devices = devices;
            _monitoring = monitoring;
            _settings = settings;
            _email = email;
            _db = db;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var lastStatus = new Dictionary<int, bool>();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var pollCfg = await _settings.GetPollingAsync(stoppingToken);
                    var devices = await _devices.GetAllAsync(stoppingToken);
                    foreach (var d in devices)
                    {
                        var stats = await _monitoring.GetLiveStatsAsync(d, stoppingToken);
                        // Save basic time-series aggregates (bandwidth/latency)
                        await _db.ExecuteAsync("INSERT INTO traffic_stats (device_id, ts_utc, down_mbps, up_mbps, latency_ms, online) VALUES (@device_id, @ts, @down, @up, @latency, @online)", new
                        {
                            device_id = d.Id,
                            ts = DateTime.UtcNow,
                            down = stats.DownloadMbps,
                            up = stats.UploadMbps,
                            latency = stats.LatencyMs,
                            online = stats.Online
                        }, stoppingToken);

                        if (lastStatus.TryGetValue(d.Id, out var wasOnline))
                        {
                            if (wasOnline && !stats.Online)
                            {
                                await NotifyOfflineAsync(d, stoppingToken);
                            }
                        }
                        lastStatus[d.Id] = stats.Online;
                    }
                    var waitSeconds = Math.Max(3, pollCfg.GlobalIntervalSeconds);
                    await Task.Delay(TimeSpan.FromSeconds(waitSeconds), stoppingToken);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Polling loop error");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task NotifyOfflineAsync(DeviceRecord d, CancellationToken ct)
        {
            var timestamp = DateTime.UtcNow.ToString("u");
            var subject = $"PulsNet: Device Offline – {d.ClientName} ({d.CircuitNumber})";
            var body = $"Device offline at {timestamp}<br/>Client: {d.ClientName}<br/>Circuit: {d.CircuitNumber}";
            // In real setup, use a helpdesk distribution list from settings; here we reuse SMTP user
            // Adjust as needed in deployment README
            var to = "helpdesk@example.com";
            try { await _email.SendAsync(to, subject, body, ct); } catch { /* ignore email errors */ }
        }
    }
}

