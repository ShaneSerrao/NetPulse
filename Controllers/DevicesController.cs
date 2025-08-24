using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Services;
using PulsNet.Data;

namespace PulsNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class DevicesController : ControllerBase
    {
        private readonly DeviceService _devs;
        private readonly MonitoringService _mon;
        private readonly Db _db;

        public DevicesController(DeviceService devs, MonitoringService mon, Db db)
        {
            _devs = devs;
            _mon = mon;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> All([FromQuery] int? tenantId)
        {
            if (tenantId.HasValue)
            {
                var list = await _db.Many(
                    @"SELECT d.id,d.client_name,d.circuit_number,d.ip_address,d.snmp_community,d.max_link_mbps,d.per_client_interval_seconds
                      FROM devices d
                      JOIN device_tenants dt ON dt.device_id = d.id
                      WHERE dt.tenant_id=@t
                      ORDER BY d.client_name",
                    r => new PulsNet.Services.DeviceService.Device
                    {
                        Id = r.GetInt32(0),
                        ClientName = r.GetString(1),
                        Circuit = r.GetString(2),
                        Ip = r.GetString(3),
                        Comm = r.GetString(4),
                        Max = r.GetInt32(5),
                        Interval = r.IsDBNull(6) ? null : r.GetInt32(6)
                    },
                    new { t = tenantId.Value }
                );
                return Ok(list);
            }

            return Ok(await _devs.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> One(int id)
        {
            var d = await _devs.Get(id);
            return d == null ? NotFound() : Ok(d);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PulsNet.Services.DeviceService.Device d)
        {
            var id = await _devs.Create(d);
            return Ok(new { id });
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PulsNet.Services.DeviceService.Device d)
        {
            d.Id = id;
            await _devs.Update(d);
            return Ok();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _devs.Delete(id);
            return Ok();
        }

        [HttpGet("{id}/live")]
        public async Task<IActionResult> Live(int id)
        {
            var d = await _devs.Get(id);
            if (d == null) return NotFound();

            // interface_index and caps read from v0.7 extensions when wired (default 1)
            var res = await _mon.LiveStats(id, d.Ip, d.Comm, d.Max, 1);
            return Ok(res);
        }
    }
}
