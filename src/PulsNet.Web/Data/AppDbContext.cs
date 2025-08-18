using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Models;

namespace PulsNet.Web.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Device> Devices => Set<Device>();
        public DbSet<TrafficSample> TrafficSamples => Set<TrafficSample>();
    }
}