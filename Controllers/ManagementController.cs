using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Services;

namespace PulsNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public sealed class ManagementController : ControllerBase
    {
        private readonly DeviceManagementService _svc;

        public ManagementController(DeviceManagementService svc)
        {
            _svc = svc;
        }

        public sealed class ApplyBody { public int TemplateId { get; set; } public int[] DeviceIds { get; set; } = Array.Empty<int>(); }
        [HttpPost("apply-template")]
        public async Task<IActionResult> Apply([FromBody] ApplyBody b)
        {
            var id = await _svc.Enqueue("ApplyTemplate", GetUserId(), b.DeviceIds, new { templateId = b.TemplateId });
            return Ok(new { actionId = id });
        }

        public sealed class RunScriptBody { public int ScriptId { get; set; } public int[] DeviceIds { get; set; } = Array.Empty<int>(); }
        [HttpPost("run-script")]
        public async Task<IActionResult> RunScript([FromBody] RunScriptBody b)
        {
            var id = await _svc.Enqueue("RunScript", GetUserId(), b.DeviceIds, new { scriptId = b.ScriptId });
            return Ok(new { actionId = id });
        }

        public sealed class FirmwareBody { public string FirmwareVersion { get; set; } = "!"; public int[] DeviceIds { get; set; } = Array.Empty<int>(); }
        [HttpPost("firmware")]
        public async Task<IActionResult> Firmware([FromBody] FirmwareBody b)
        {
            var id = await _svc.Enqueue("FirmwareUpdate", GetUserId(), b.DeviceIds, new { version = b.FirmwareVersion, staged = true });
            return Ok(new { actionId = id });
        }

        public sealed class UpdateCfgBody { public string ChangeType { get; set; } = "!"; public object? Payload { get; set; } public int[] DeviceIds { get; set; } = Array.Empty<int>(); }
        [HttpPost("update-config")]
        public async Task<IActionResult> UpdateConfig([FromBody] UpdateCfgBody b)
        {
            var id = await _svc.Enqueue("UpdateConfig", GetUserId(), b.DeviceIds, new { changeType = b.ChangeType, payload = b.Payload });
            return Ok(new { actionId = id });
        }

        public sealed class RollbackBody { public long ActionId { get; set; } }
        [HttpPost("rollback")]
        public async Task<IActionResult> Rollback([FromBody] RollbackBody b)
        {
            var id = await _svc.Enqueue("Rollback", GetUserId(), Array.Empty<int>(), new { actionId = b.ActionId });
            return Ok(new { actionId = id });
        }

        // --- Added status endpoint ---
        [HttpGet("{actionId:long}")]
        public async Task<IActionResult> Status(long actionId, [FromServices] PulsNet.Data.Db db)
        {
            var st = await db.One(
                "SELECT status, progress_percent, error FROM management_actions WHERE id=@id",
                r => new
                {
                    status = r.GetString(0),
                    progress = r.GetInt32(1),
                    error = r.IsDBNull(2) ? null : r.GetString(2)
                },
                new { id = actionId }
            );

            return st == null ? NotFound() : Ok(st);
        }

        private int GetUserId()
        {
            var s = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(s, out var v) ? v : 0;
        }
    }
}
