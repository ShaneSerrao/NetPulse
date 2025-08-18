using System.Text.Json;

namespace PulsNet.Services
{
    public sealed class ConfigService
    {
        public AppConfig Config { get; }

        public ConfigService()
        {
            var configPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../config/secure_config.json"));
            if (!File.Exists(configPath))
            {
                // Fallback to copied content if running published
                var fallback = Path.Combine(AppContext.BaseDirectory, "secure_config.json");
                configPath = File.Exists(fallback) ? fallback : configPath;
            }

            using var stream = File.OpenRead(configPath);
            var cfg = JsonSerializer.Deserialize<AppConfig>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Config = cfg ?? throw new InvalidOperationException("secure_config.json missing or invalid");
        }
    }

    public sealed class AppConfig
    {
        public DatabaseConfig Database { get; set; } = new();
        public SecurityConfig Security { get; set; } = new();
        public SmtpConfig Smtp { get; set; } = new();
        public PollingConfig Polling { get; set; } = new();
        public ThemeConfig Theme { get; set; } = new();
    }

    public sealed class DatabaseConfig
    {
        public string Provider { get; set; } = "PostgreSQL";
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = "pulsnet";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class SecurityConfig
    {
        public bool Global2FAEnabled { get; set; } = true;
        public BruteForceConfig BruteForceProtection { get; set; } = new();
        public string[] AllowedSnmpVersions { get; set; } = new[] { "2c" };
        public bool HttpsOnly { get; set; } = true;
    }

    public sealed class BruteForceConfig
    {
        public int MaxAttempts { get; set; } = 5;
        public int LockoutMinutes { get; set; } = 15;
    }

    public sealed class SmtpConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public bool UseStartTls { get; set; } = true;
    }

    public sealed class PollingConfig
    {
        public int GlobalIntervalSeconds { get; set; } = 10;
        public int CacheSeconds { get; set; } = 5;
        public int OfflineThresholdSeconds { get; set; } = 60;
    }

    public sealed class ThemeConfig
    {
        public string Primary { get; set; } = "#0ea5e9";
        public string Accent { get; set; } = "#22c55e";
        public string Warning { get; set; } = "#f59e0b";
        public string Danger { get; set; } = "#ef4444";
        public string Background { get; set; } = "#0b1220";
        public string Surface { get; set; } = "#0f172a";
        public string Text { get; set; } = "#e5e7eb";
    }
}

