using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulsNet.Web.Models
{
    public class Device
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CircuitNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        [MaxLength(128)]
        public string SnmpCommunity { get; set; } = string.Empty;

        public int SnmpPort { get; set; } = 161;
        public int MaxLinkMbps { get; set; } = 1000;

        public int? PollIntervalSecondsOverride { get; set; }
        public DateTimeOffset? PollIntervalOverrideSetAt { get; set; }

        [ForeignKey("Tenant")] public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        [MaxLength(32)] public string? LineSpeedLabel { get; set; } // e.g., "100/50 Mbps"

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [MaxLength(16)] public string CardSize { get; set; } = "md"; // sm/md/lg
    }
}