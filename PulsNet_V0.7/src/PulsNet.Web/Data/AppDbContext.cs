using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Models;

namespace PulsNet.Web.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Device> Devices => Set<Device>();
        public DbSet<Interface> Interfaces => Set<Interface>();
        public DbSet<TrafficSample> TrafficSamples => Set<TrafficSample>();
        public DbSet<AppSettings> AppSettings => Set<AppSettings>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Mib> Mibs => Set<Mib>();
        public DbSet<MibOid> MibOids => Set<MibOid>();
        public DbSet<DeviceMib> DeviceMibs => Set<DeviceMib>();
        public DbSet<ThresholdRule> ThresholdRules => Set<ThresholdRule>();
        public DbSet<NotificationChannel> NotificationChannels => Set<NotificationChannel>();
        public DbSet<Incident> Incidents => Set<Incident>();
        public DbSet<DeviceUser> DeviceUsers => Set<DeviceUser>();
    }
}