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

        public DbSet<ConfigTemplate> ConfigTemplates => Set<ConfigTemplate>();
        public DbSet<ConfigTemplateVersion> ConfigTemplateVersions => Set<ConfigTemplateVersion>();
        public DbSet<DeviceConfigAssignment> DeviceConfigAssignments => Set<DeviceConfigAssignment>();
        public DbSet<ScriptItem> ScriptItems => Set<ScriptItem>();
        public DbSet<ScriptExecution> ScriptExecutions => Set<ScriptExecution>();
        public DbSet<FirmwareCatalog> FirmwareCatalogs => Set<FirmwareCatalog>();
        public DbSet<FirmwareDeployment> FirmwareDeployments => Set<FirmwareDeployment>();
        public DbSet<InterfaceChangeSet> InterfaceChangeSets => Set<InterfaceChangeSet>();
        public DbSet<ConfigHistory> ConfigHistories => Set<ConfigHistory>();
    }
}