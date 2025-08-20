using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Data;
using PulsNet.Web.Models;

namespace PulsNet.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Devices()
        {
            var devices = await _db.Devices.Include(d=>d.Tenant).AsNoTracking().OrderBy(d => d.ClientName).ToListAsync();
            ViewBag.Tenants = await _db.Tenants.AsNoTracking().OrderBy(t=>t.Name).ToListAsync();
            return View(devices);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDevice()
        {
            ViewBag.Tenants = await _db.Tenants.AsNoTracking().OrderBy(t=>t.Name).ToListAsync();
            return View(new Device());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDevice(Device device)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tenants = await _db.Tenants.AsNoTracking().OrderBy(t=>t.Name).ToListAsync();
                return View(device);
            }
            _db.Devices.Add(device);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Devices));
        }

        [HttpGet]
        public async Task<IActionResult> EditDevice(int id)
        {
            var dev = await _db.Devices.FindAsync(id);
            if (dev == null) return NotFound();
            ViewBag.Tenants = await _db.Tenants.AsNoTracking().OrderBy(t=>t.Name).ToListAsync();
            return View(dev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDevice(Device device)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tenants = await _db.Tenants.AsNoTracking().OrderBy(t=>t.Name).ToListAsync();
                return View(device);
            }
            _db.Update(device);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Devices));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var dev = await _db.Devices.FindAsync(id);
            if (dev != null)
            {
                _db.Devices.Remove(dev);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Devices));
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var s = await _db.AppSettings.FirstOrDefaultAsync() ?? new AppSettings();
            if (s.Id == 0) { _db.AppSettings.Add(s); await _db.SaveChangesAsync(); }
            return View(s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(AppSettings settings)
        {
            if (!ModelState.IsValid) return View(settings);
            if (settings.Id == 0) _db.AppSettings.Add(settings); else _db.AppSettings.Update(settings);
            await _db.SaveChangesAsync();
            TempData["Saved"] = true;
            return RedirectToAction(nameof(Settings));
        }
    }
}