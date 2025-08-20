using System.ComponentModel.DataAnnotations.Schema;

namespace PulsNet.Web.Models
{
    public class DeviceUser
    {
        public int Id { get; set; }
        [ForeignKey("Device")] public int DeviceId { get; set; }
        public Device? Device { get; set; }
        [ForeignKey("User")] public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public string AccessLevel { get; set; } = "Viewer"; // Viewer/Operator/Admin
    }

    public class DeviceMib
    {
        public int Id { get; set; }
        [ForeignKey("Device")] public int DeviceId { get; set; }
        public Device? Device { get; set; }
        [ForeignKey("Mib")] public int MibId { get; set; }
        public Mib? Mib { get; set; }
    }
}