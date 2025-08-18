using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Services;

namespace PulsNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class DevicesController : ControllerBase
    {
        private readonly DeviceService _devices;
        private readonly MonitoringService _monitoring;

        public DevicesController(DeviceService devices, MonitoringService monitoring)
        {
            _devices = devices;
            _monitoring = monitoring;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var all = await _devices.GetAllAsync(ct);
            return Ok(all);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken ct)
        {
            var d = await _devices.GetByIdAsync(id, ct);
            if (d == null) return NotFound();
            return Ok(d);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DeviceRecord device, CancellationToken ct)
        {
            var id = await _devices.CreateAsync(device, ct);
            return Ok(new { id });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DeviceRecord device, CancellationToken ct)
        {
            device.Id = id;
            await _devices.UpdateAsync(device, ct);
            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _devices.DeleteAsync(id, ct);
            return Ok();
        }

        [HttpGet("{id}/live")]
        public async Task<IActionResult> Live(int id, CancellationToken ct)
        {
            var d = await _devices.GetByIdAsync(id, ct);
            if (d == null) return NotFound();
            var stats = await _monitoring.GetLiveStatsAsync(d, ct);
            return Ok(stats);
        }
    }
}

