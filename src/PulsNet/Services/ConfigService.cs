using System.Text.Json;

namespace PulsNet.Services
{
    public sealed class ConfigService
    {
        public AppConfig Config { get; }

        public ConfigService()
        {
            var envPath = Environment.GetEnvironmentVariable("PULSNET_SECRETS_PATH");
            string? resolved = null;

            // 1️⃣ Check environment variable
            if (!string.IsNullOrWhiteSpace(envPath) && File.Exists(envPath))
            {
                resolved = envPath;
            }
            else
            {
                // 2️⃣ Check project-relative (copied to output)
                var local = Path.Combine(AppContext.BaseDirectory, "secure_config.json");
                if (File.Exists(local)) resolved = local;

                // 3️⃣ Check repository config
                if (resolved == null)
                {
                    var repoPath = Path.GetFullPath(
                        Path.Combine(AppContext.BaseDirectory, "../../../../../config/secure_config.json")
                    );
                    if (File.Exists(repoPath)) resolved = repoPath;
                }
            }

            // 4️⃣ Throw if not found
            if (resolved == null)
            {
                throw new FileNotFoundException(
                    "secure_config.json not found. Set PULSNET_SECRETS_PATH or ensure config/secure_config.json exists and is copied to output."
                );
            }

            // 5️⃣ Load config
            using var s = File.OpenRead(resolved);
            Config = JsonSerializer.Deserialize<AppConfig>(
                s,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
        }
    }

    public sealed class AppConfig
    {
        public DbConfig Database { get; set; } = new();
        public SecConfig Security { get; set; } = new();
        public PollConfig Polling { get; set; } = new();
        public ThemeConfig Theme { get; set; } = new();
        public SmtpConfig Smtp { get; set; } = new();
    }

    public sealed class DbConfig
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public sealed class SecConfig
    {
        public bool Global2FAEnabled { get; set; } = false;
        public bool HttpsOnly { get; set; } = true;
    }

    public sealed class PollConfig
    {
        public int GlobalIntervalSeconds { get; set; } = 5;
        public int CacheSeconds { get; set; } = 5;
        public int OfflineThresholdSeconds { get; set; } = 15;
    }

    public sealed class ThemeConfig
    {
        public string Name { get; set; } = "dark";
        public string Primary { get; set; } = "#2a3867";
        public string Accent { get; set; } = "#c49014";
    }

    public sealed class SmtpConfig
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
