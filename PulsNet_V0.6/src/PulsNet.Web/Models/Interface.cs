using System.ComponentModel.DataAnnotations.Schema;

namespace PulsNet.Web.Models
{
    public class Interface
    {
        public int Id { get; set; }
        [ForeignKey("Device")] public int DeviceId { get; set; }
        public Device? Device { get; set; }
        public string Name { get; set; } = string.Empty;
        public long SpeedMbps { get; set; }
        public string Type { get; set; } = "ethernet"; // ethernet/sfp/vlan/wlan/vpn
        public bool Enabled { get; set; } = true;
    }
}