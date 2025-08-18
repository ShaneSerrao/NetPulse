using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Data;

namespace PulsNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class OfflineController : ControllerBase
    {
        private readonly Db _db;

        public OfflineController(Db db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            const string sql = @"WITH latest AS (
                SELECT DISTINCT ON (device_id) device_id, online
                FROM traffic_stats
                ORDER BY device_id, ts_utc DESC
            )
            SELECT d.id, d.client_name, d.circuit_number, d.ip_address
            FROM devices d
            JOIN latest l ON l.device_id = d.id
            WHERE l.online = false";
            var list = await _db.QueryAsync(sql, r => new
            {
                id = r.GetInt32(0),
                clientName = r.GetString(1),
                circuitNumber = r.GetString(2),
                ip = r.GetString(3)
            }, null, ct);
            return Ok(list);
        }
    }
}

