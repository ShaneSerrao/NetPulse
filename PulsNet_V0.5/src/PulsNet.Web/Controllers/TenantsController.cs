using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Data;
using PulsNet.Web.Models;

namespace PulsNet.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TenantsController : Controller
    {
        private readonly AppDbContext _db;
        public TenantsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Index() => View(await _db.Tenants.AsNoTracking().ToListAsync());

        [HttpGet]
        public IActionResult Create() => View(new Tenant());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tenant t)
        {
            if (!ModelState.IsValid) return View(t);
            _db.Tenants.Add(t); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var t = await _db.Tenants.FindAsync(id); if (t == null) return NotFound();
            return View(t);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tenant t)
        {
            if (!ModelState.IsValid) return View(t);
            _db.Update(t); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.Tenants.FindAsync(id); if (t != null) { _db.Remove(t); await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}