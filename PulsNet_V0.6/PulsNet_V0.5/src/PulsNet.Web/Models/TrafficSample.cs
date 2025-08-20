using System.ComponentModel.DataAnnotations.Schema;

namespace PulsNet.Web.Models
{
    public class TrafficSample
    {
        public int Id { get; set; }
        [ForeignKey("Device")] public int DeviceId { get; set; }
        public Device? Device { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public double DownloadMbps { get; set; }
        public double UploadMbps { get; set; }
        public int LatencyMs { get; set; }
        public bool IsOnline { get; set; }
    }
}