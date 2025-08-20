namespace PulsNet.Web.Models
{
    public class ThresholdRule
    {
        public int Id { get; set; }
        public int? DeviceId { get; set; }
        public int? InterfaceId { get; set; }
        public string Metric { get; set; } = string.Empty; // e.g., cpu, ram, ifUtil, latency
        public double GreaterThan { get; set; }
        public int DurationSeconds { get; set; } = 60;
        public bool Enabled { get; set; } = true;
    }

    public class NotificationChannel
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string Type { get; set; } = "email"; // email/telegram/slack/sms/webhook
        public string Target { get; set; } = string.Empty; // address/url/phone
        public bool Enabled { get; set; } = true;
    }

    public class Incident
    {
        public int Id { get; set; }
        public int? DeviceId { get; set; }
        public string Type { get; set; } = string.Empty; // up/down/threshold
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
    }
}