using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulsNet.Web.Models
{
    public class ConfigTemplate
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        [MaxLength(1000)] public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;
        [ForeignKey("Tenant")] public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class ConfigTemplateVersion
    {
        public int Id { get; set; }
        [ForeignKey("Template")] public int TemplateId { get; set; }
        public ConfigTemplate? Template { get; set; }
        public int Version { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? CreatedByUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class DeviceConfigAssignment
    {
        public int Id { get; set; }
        [ForeignKey("Device")] public int DeviceId { get; set; }
        public Device? Device { get; set; }
        [ForeignKey("Template")] public int TemplateId { get; set; }
        public ConfigTemplate? Template { get; set; }
        public string VariablesJson { get; set; } = "{}"; // key/value
    }

    public class ScriptItem
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        [MaxLength(1000)] public string? Description { get; set; }
        public string Content { get; set; } = string.Empty; // RouterOS script
    }

    public class ScriptExecution
    {
        public int Id { get; set; }
        public int ScriptId { get; set; }
        public ScriptItem? Script { get; set; }
        public int DeviceId { get; set; }
        public Device? Device { get; set; }
        public string ExecutedByUserId { get; set; } = string.Empty;
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled/Running/Success/Failed
        public string Output { get; set; } = string.Empty;
    }

    public class FirmwareCatalog
    {
        public int Id { get; set; }
        [Required, MaxLength(50)] public string Version { get; set; } = string.Empty;
        [MaxLength(500)] public string? Url { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
    }

    public class FirmwareDeployment
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public Device? Device { get; set; }
        public string FirmwareVersion { get; set; } = string.Empty;
        public DateTimeOffset ScheduledAt { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled/Running/Success/Failed/RolledBack
        public string Log { get; set; } = string.Empty;
    }

    public class InterfaceChangeSet
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public Device? Device { get; set; }
        public string ChangesJson { get; set; } = "{}"; // structured description of VLAN/Queues/VPN/Interfaces
        public string RequestedByUserId { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string Status { get; set; } = "Scheduled";
    }

    public class ConfigHistory
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public Device? Device { get; set; }
        [MaxLength(50)] public string ActionType { get; set; } = string.Empty; // Template/Script/Firmware/Interface/Rollback
        public string UserId { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public string OldConfig { get; set; } = string.Empty;
        public string NewConfig { get; set; } = string.Empty;
        public string Status { get; set; } = "Success"; // Success/Failed
        public string Message { get; set; } = string.Empty;
    }
}