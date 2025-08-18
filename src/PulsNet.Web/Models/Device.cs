using System.ComponentModel.DataAnnotations;

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

        public bool IsOnline { get; set; }

        public int? PollIntervalSecondsOverride { get; set; }
        public DateTimeOffset? PollIntervalOverrideSetAt { get; set; }

        public long MonthlyBytesTransferred { get; set; }

        public int MaxLinkMbps { get; set; } = 1000;
    }
}