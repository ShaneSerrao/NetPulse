using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PulsNet.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public sealed class StatsController : ControllerBase {
    [HttpGet("{deviceId:int}/monthly-usage")]
    public IActionResult MonthlyUsage(int deviceId){
      // Placeholder: return 30 days of zeroed usage to satisfy UI until real metrics are wired
      var today = DateTime.UtcNow.Date;
      var points = Enumerable.Range(0, 30).Select(i => new {
        date = today.AddDays(-i).ToString("yyyy-MM-dd"),
        downloadMbps = 0,
        uploadMbps = 0
      }).Reverse().ToArray();
      return Ok(new { deviceId, points });
    }
  }
}

