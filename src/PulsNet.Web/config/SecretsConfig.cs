using System.Text.Json;

namespace PulsNet.Web.config
{
    public class Secrets
    {
        public DatabaseSecrets Db { get; set; } = new DatabaseSecrets();
        public string HelpdeskEmail { get; set; } = "helpdesk@example.com";
        public int GlobalPollIntervalSeconds { get; set; } = 5;
        public bool GlobalTwoFactorEnabled { get; set; } = false;
    }

    public class DatabaseSecrets
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = "pulsnet";
        public string Username { get; set; } = "pulsnet";
        public string Password { get; set; } = "changeme";
    }

    public static class SecretsLoader
    {
        public static Secrets Load(string? path = null)
        {
            var secretsPath = path ?? Environment.GetEnvironmentVariable("PULSNET_SECRETS_PATH") ?? "/etc/pulsnet/pulsnet.secrets.json";
            if (!File.Exists(secretsPath)) return new Secrets();
            var json = File.ReadAllText(secretsPath);
            return JsonSerializer.Deserialize<Secrets>(json, new JsonSerializerOptions{PropertyNameCaseInsensitive=true}) ?? new Secrets();
        }

        public static string BuildPostgresConnectionString(Secrets secrets)
        {
            return $"Host={secrets.Db.Host};Port={secrets.Db.Port};Database={secrets.Db.Database};Username={secrets.Db.Username};Password={secrets.Db.Password};Pooling=true;";
        }
    }
}