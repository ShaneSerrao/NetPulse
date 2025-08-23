using System.ComponentModel.DataAnnotations;

namespace PulsNet.Web.Models
{
    public class Mib
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        [MaxLength(1000)] public string? Description { get; set; }
        public List<MibOid> Oids { get; set; } = new();
    }

    public class MibOid
    {
        public int Id { get; set; }
        public int MibId { get; set; }
        public string Oid { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
    }
}