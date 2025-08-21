using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulsNet.Web.Models
{
    public class DeviceSelectedOid
    {
        public int Id { get; set; }
        [ForeignKey("Device")] public int DeviceId { get; set; }
        public Device? Device { get; set; }
        [Required] public string Oid { get; set; } = string.Empty; // e.g., IF-MIB::ifInOctets.1
        [MaxLength(200)] public string? Label { get; set; } // human-friendly label
        [MaxLength(50)] public string? Category { get; set; } // Interfaces, IP, System
    }

    public class DeviceOidSample
    {
        public int Id { get; set; }
        [ForeignKey("Device")] public int DeviceId { get; set; }
        public Device? Device { get; set; }
        [Required] public string Oid { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}