using System.Text.Json;
using PulsNet.Data;

namespace PulsNet.Services
{
    public sealed class SettingsService
    {
        private readonly Db _db;
        private readonly ConfigService _configService;

        public SettingsService(Db db, ConfigService configService)
        {
            _db = db;
            _configService = configService;
        }

        public async Task<ThemeConfig> GetThemeAsync(CancellationToken ct)
        {
            var json = await GetSettingAsync("theme", ct);
            if (string.IsNullOrWhiteSpace(json)) return _configService.Config.Theme;
            return JsonSerializer.Deserialize<ThemeConfig>(json) ?? _configService.Config.Theme;
        }

        public async Task SetThemeAsync(ThemeConfig theme, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(theme);
            await UpsertSettingAsync("theme", json, ct);
        }

        public async Task<PollingConfig> GetPollingAsync(CancellationToken ct)
        {
            var json = await GetSettingAsync("polling", ct);
            if (string.IsNullOrWhiteSpace(json)) return _configService.Config.Polling;
            return JsonSerializer.Deserialize<PollingConfig>(json) ?? _configService.Config.Polling;
        }

        public async Task SetPollingAsync(PollingConfig polling, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(polling);
            await UpsertSettingAsync("polling", json, ct);
        }

        public async Task<bool> GetGlobal2FAEnabledAsync(CancellationToken ct)
        {
            var json = await GetSettingAsync("global2fa", ct);
            if (string.IsNullOrWhiteSpace(json)) return _configService.Config.Security.Global2FAEnabled;
            return bool.TryParse(json, out var b) ? b : _configService.Config.Security.Global2FAEnabled;
        }

        public async Task SetGlobal2FAEnabledAsync(bool enabled, CancellationToken ct)
        {
            await UpsertSettingAsync("global2fa", enabled.ToString(), ct);
        }

        private async Task<string?> GetSettingAsync(string key, CancellationToken ct)
        {
            const string sql = "SELECT value_json FROM settings WHERE key=@key";
            return await _db.QuerySingleAsync(sql, r => r.GetString(0), new { key }, ct);
        }

        private async Task UpsertSettingAsync(string key, string json, CancellationToken ct)
        {
            const string sql = @"INSERT INTO settings(key, value_json) VALUES (@key, @json)
                                ON CONFLICT (key) DO UPDATE SET value_json = EXCLUDED.value_json";
            await _db.ExecuteAsync(sql, new { key, json }, ct);
        }
    }
}

