using System.ComponentModel.DataAnnotations;

namespace PulsNet.Web.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        [MaxLength(400)] public string? Description { get; set; }
    }
}