using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Data;
using PulsNet.Web.Models;

namespace PulsNet.Web.Services
{
    public class DeviceManagementService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DeviceManagementService> _logger;

        public DeviceManagementService(AppDbContext db, ILogger<DeviceManagementService> logger)
        {
            _db = db; _logger = logger;
        }

        public async Task<(bool ok, string message)> ApplyTemplate(IEnumerable<int> deviceIds, int templateId, string userId, CancellationToken ct)
        {
            var template = await _db.Set<ConfigTemplate>().FindAsync(new object?[]{templateId}, ct);
            if (template == null) return (false, "Template not found");
            foreach (var deviceId in deviceIds)
            {
                await Audit(deviceId, "Template", userId, oldConfig: "", newConfig: template.Content, status: "Scheduled", message: "Apply template");
                // TODO: render variables & push via RouterOS API/SSH
            }
            await _db.SaveChangesAsync(ct);
            return (true, "Template scheduled for application");
        }

        public async Task<(bool ok, string message)> RunScript(IEnumerable<int> deviceIds, int scriptId, string userId, CancellationToken ct)
        {
            var script = await _db.Set<ScriptItem>().FindAsync(new object?[]{scriptId}, ct);
            if (script == null) return (false, "Script not found");
            foreach (var deviceId in deviceIds)
            {
                var exec = new ScriptExecution{ ScriptId = scriptId, DeviceId = deviceId, ExecutedByUserId = userId, StartedAt = DateTimeOffset.UtcNow, Status = "Scheduled" };
                _db.Add(exec);
                await Audit(deviceId, "Script", userId, oldConfig: "", newConfig: script.Content, status: "Scheduled", message: "Execute script");
            }
            await _db.SaveChangesAsync(ct);
            return (true, "Script executions scheduled");
        }

        public async Task<(bool ok, string message)> FirmwareUpdate(IEnumerable<int> deviceIds, string version, string userId, CancellationToken ct)
        {
            foreach (var deviceId in deviceIds)
            {
                _db.Add(new FirmwareDeployment{ DeviceId = deviceId, FirmwareVersion = version, ScheduledAt = DateTimeOffset.UtcNow, Status = "Scheduled" });
                await Audit(deviceId, "Firmware", userId, oldConfig: "", newConfig: version, status: "Scheduled", message: "Firmware update");
            }
            await _db.SaveChangesAsync(ct);
            return (true, "Firmware updates scheduled");
        }

        public async Task<(bool ok, string message)> UpdateInterface(IEnumerable<int> deviceIds, string changesJson, string userId, CancellationToken ct)
        {
            foreach (var deviceId in deviceIds)
            {
                _db.Add(new InterfaceChangeSet{ DeviceId = deviceId, ChangesJson = changesJson, RequestedByUserId = userId, Status = "Scheduled" });
                await Audit(deviceId, "Interface", userId, oldConfig: "", newConfig: changesJson, status: "Scheduled", message: "Interface/VLAN/Queue/VPN update");
            }
            await _db.SaveChangesAsync(ct);
            return (true, "Interface changes scheduled");
        }

        public async Task<(bool ok, string message)> Rollback(int deviceId, int actionId, string userId, CancellationToken ct)
        {
            var h = await _db.Set<ConfigHistory>().FirstOrDefaultAsync(x => x.Id == actionId && x.DeviceId == deviceId, ct);
            if (h == null) return (false, "Action not found");
            // TODO: push h.OldConfig back
            await Audit(deviceId, "Rollback", userId, oldConfig: h.NewConfig, newConfig: h.OldConfig, status: "Scheduled", message: $"Rollback of {actionId}");
            await _db.SaveChangesAsync(ct);
            return (true, "Rollback scheduled");
        }

        public async Task Audit(int deviceId, string actionType, string userId, string oldConfig, string newConfig, string status, string message)
        {
            _db.Add(new ConfigHistory
            {
                DeviceId = deviceId,
                ActionType = actionType,
                UserId = userId,
                Timestamp = DateTimeOffset.UtcNow,
                OldConfig = oldConfig,
                NewConfig = newConfig,
                Status = status,
                Message = message
            });
            await Task.CompletedTask;
        }
    }
}