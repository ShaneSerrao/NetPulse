using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Data;
using PulsNet.Web.Models;

namespace PulsNet.Web.Controllers
{
    [Authorize(Roles = "Admin,Operator")]
    public class MonitoringController : Controller
    {
        private readonly AppDbContext _db;
        public MonitoringController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Thresholds() => View(await _db.ThresholdRules.AsNoTracking().ToListAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThreshold(ThresholdRule r)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Thresholds));
            _db.ThresholdRules.Add(r); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Thresholds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteThreshold(int id)
        {
            var r = await _db.ThresholdRules.FindAsync(id); if (r!=null){_db.Remove(r); await _db.SaveChangesAsync();}
            return RedirectToAction(nameof(Thresholds));
        }

        [HttpGet]
        public async Task<IActionResult> Notifications() => View(await _db.NotificationChannels.AsNoTracking().ToListAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNotification(NotificationChannel c)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Notifications));
            _db.NotificationChannels.Add(c); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Notifications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var c = await _db.NotificationChannels.FindAsync(id); if (c!=null){_db.Remove(c); await _db.SaveChangesAsync();}
            return RedirectToAction(nameof(Notifications));
        }

        [HttpGet]
        public async Task<IActionResult> History() => View(await _db.Incidents.AsNoTracking().OrderByDescending(i=>i.Timestamp).Take(1000).ToListAsync());
    }
}