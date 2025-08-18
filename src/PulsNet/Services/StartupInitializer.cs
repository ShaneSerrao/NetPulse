using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PulsNet.Data;

namespace PulsNet.Services
{
    public sealed class StartupInitializer : IHostedService
    {
        private readonly ILogger<StartupInitializer> _logger;
        private readonly Db _db;
        private readonly SettingsService _settings;

        public StartupInitializer(ILogger<StartupInitializer> logger, Db db, SettingsService settings)
        {
            _logger = logger;
            _db = db;
            _settings = settings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var count = await _db.QuerySingleAsync("SELECT COUNT(1) FROM users", r => r.GetInt64(0), null, cancellationToken) ?? 0;
                if (count == 0)
                {
                    var (salt, hash) = AuthService.HashPassword("admin123");
                    await _db.ExecuteAsync("INSERT INTO users (username, role, password_hash, password_salt) VALUES ('admin','Admin',@hash,@salt)", new { hash, salt }, cancellationToken);
                    _logger.LogWarning("No users found. Created default admin user with username 'admin' and password 'admin123'. CHANGE THIS IMMEDIATELY.");
                }

                var theme = await _settings.GetThemeAsync(cancellationToken);
                await _settings.SetThemeAsync(theme, cancellationToken);
                var polling = await _settings.GetPollingAsync(cancellationToken);
                await _settings.SetPollingAsync(polling, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Startup initialization failed");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PulsNet.Data;

namespace PulsNet.Services
{
    public sealed class StartupInitializer : IHostedService
    {
        private readonly ILogger<StartupInitializer> _logger;
        private readonly Db _db;
        private readonly SettingsService _settings;

        public StartupInitializer(ILogger<StartupInitializer> logger, Db db, SettingsService settings)
        {
            _logger = logger;
            _db = db;
            _settings = settings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var count = await _db.QuerySingleAsync("SELECT COUNT(1) FROM users", r => r.GetInt64(0), null, cancellationToken) ?? 0;
                if (count == 0)
                {
                    var (salt, hash) = AuthService.HashPassword("admin123");
                    await _db.ExecuteAsync("INSERT INTO users (username, role, password_hash, password_salt) VALUES ('admin','Admin',@hash,@salt)", new { hash, salt }, cancellationToken);
                    _logger.LogWarning("No users found. Created default admin user with username 'admin' and password 'admin123'. CHANGE THIS IMMEDIATELY.");
                }

                // Ensure theme/polling settings exist (defaults from secure_config.json)
                var theme = await _settings.GetThemeAsync(cancellationToken);
                await _settings.SetThemeAsync(theme, cancellationToken);
                var polling = await _settings.GetPollingAsync(cancellationToken);
                await _settings.SetPollingAsync(polling, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Startup initialization failed");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

