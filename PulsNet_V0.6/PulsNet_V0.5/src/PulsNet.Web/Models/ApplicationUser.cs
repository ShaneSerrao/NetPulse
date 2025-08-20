using Microsoft.AspNetCore.Identity;

namespace PulsNet.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool TwoFactorPreference { get; set; } = false;
    }
}