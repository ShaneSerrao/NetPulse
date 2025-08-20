using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PulsNet.Web.Data;
using PulsNet.Web.Services;

namespace PulsNet.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ServerMetricsService _serverMetrics;

        public DashboardController(AppDbContext db, IMemoryCache cache, ServerMetricsService serverMetrics)
        {
            _db = db;
            _cache = cache;
            _serverMetrics = serverMetrics;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var devices = await _db.Devices.AsNoTracking().Include(d=>d.Tenant).OrderBy(d => d.ClientName).ToListAsync();
            return View(devices);
        }

        [HttpGet]
        public async Task<IActionResult> LiveSample(int id)
        {
            if (_cache.TryGetValue($"device:{id}:lastSample", out object? value) && value != null)
            {
                return Json(value);
            }

            var sample = await _db.TrafficSamples.AsNoTracking().Where(t => t.DeviceId == id)
                .OrderByDescending(t => t.Timestamp).FirstOrDefaultAsync();
            if (sample == null) return NotFound();
            return Json(sample);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Operator,SuperAdmin")]
        public IActionResult ServerMetrics()
        {
            var metrics = _serverMetrics.GetCurrent();
            return Json(metrics);
        }
    }
}