namespace PulsNet.Web.Models
{
    public class AppSettings
    {
        public int Id { get; set; }
        public bool GlobalTwoFactorEnabled { get; set; } = false;
        public int GlobalPollIntervalSeconds { get; set; } = 5;
        public string PrimaryColor { get; set; } = "#2a3867";
        public string AccentColor { get; set; } = "#aeb9ff";
        public string Theme { get; set; } = "dark";
    }
}