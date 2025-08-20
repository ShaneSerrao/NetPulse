using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulsNet.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool TwoFactorPreference { get; set; } = false;
        public int? IdleTimeoutMinutes { get; set; } // per-user auto logout

        [ForeignKey("Tenant")] public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        // Feature flags
        public bool CanViewTopology { get; set; } = true;
        public bool CanDeployConfigs { get; set; } = false;
        public bool CanRunBulkOps { get; set; } = false;
    }
}