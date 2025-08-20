using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Data;
using PulsNet.Web.Models;

namespace PulsNet.Web.Controllers
{
    [Authorize(Roles = "Admin,Operator")]
    public class MibsController : Controller
    {
        private readonly AppDbContext _db;
        public MibsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _db.Mibs.Include(m => m.Oids).AsNoTracking().ToListAsync();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create() => View(new Mib());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mib mib)
        {
            if (!ModelState.IsValid) return View(mib);
            _db.Mibs.Add(mib); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var mib = await _db.Mibs.Include(m => m.Oids).FirstOrDefaultAsync(m => m.Id == id);
            if (mib == null) return NotFound();
            return View(mib);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Mib mib)
        {
            if (!ModelState.IsValid) return View(mib);
            _db.Update(mib); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var mib = await _db.Mibs.FindAsync(id);
            if (mib != null) { _db.Remove(mib); await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOid(int mibId, string oid, string? label, string? unit, string? notes)
        {
            var mib = await _db.Mibs.FindAsync(mibId); if (mib == null) return NotFound();
            _db.MibOids.Add(new MibOid{ MibId = mibId, Oid = oid, Label = label, Unit = unit, Notes = notes });
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Edit), new { id = mibId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOid(int id, int mibId)
        {
            var o = await _db.MibOids.FindAsync(id); if (o != null) { _db.Remove(o); await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Edit), new { id = mibId });
        }
    }
}