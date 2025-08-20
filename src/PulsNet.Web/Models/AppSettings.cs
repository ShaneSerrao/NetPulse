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

        // Page customization
        public string SiteTitle { get; set; } = "PulsNet";
        public string? LogoPath { get; set; }
        public string? AltLogoPath { get; set; }
        public string Timezone { get; set; } = "UTC";
        public string GeoDefault { get; set; } = "0,0";

        // Sections toggles
        public bool EnableTopology { get; set; } = true;
        public bool EnableNetFlow { get; set; } = false;
        public bool EnableLicensing { get; set; } = false; // disabled by default

        // Dashboard preferences
        public bool DashboardShowWorldMap { get; set; } = true;
        public bool DashboardShowServerMetrics { get; set; } = true;

        // Idle logout defaults
        public int DefaultIdleTimeoutMinutes { get; set; } = 60;
    }
}