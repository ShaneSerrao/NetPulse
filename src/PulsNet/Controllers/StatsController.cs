using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Data;

namespace PulsNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class StatsController : ControllerBase
    {
        private readonly Db _db;

        public StatsController(Db db)
        {
            _db = db;
        }

        [HttpGet("{deviceId}/monthly-usage")]
        public async Task<IActionResult> GetMonthlyUsage(int deviceId, CancellationToken ct)
        {
            const string sql = @"SELECT date_trunc('month', ts_utc) AS month,
                SUM(down_mbps) AS sum_down_mbps,
                SUM(up_mbps) AS sum_up_mbps
            FROM traffic_stats
            WHERE device_id=@deviceId AND ts_utc > now() - interval '31 days'
            GROUP BY 1
            ORDER BY 1 DESC LIMIT 1";
            var res = await _db.QuerySingleAsync(sql, r => new
            {
                month = r.GetDateTime(0),
                sumDownMbps = r.IsDBNull(1) ? 0.0 : r.GetDouble(1),
                sumUpMbps = r.IsDBNull(2) ? 0.0 : r.GetDouble(2)
            }, new { deviceId }, ct);

            // Convert Mbps over seconds to bytes: assume 1-second sampling per record; approximate
            const double secondsInMonth = 30.0 * 24 * 3600;
            var downBytes = (res?.sumDownMbps ?? 0) * 1_000_000.0 / 8.0 * secondsInMonth;
            var upBytes = (res?.sumUpMbps ?? 0) * 1_000_000.0 / 8.0 * secondsInMonth;
            var totalBytes = downBytes + upBytes;
            return Ok(new { bytes = (long)totalBytes });
        }
    }
}

