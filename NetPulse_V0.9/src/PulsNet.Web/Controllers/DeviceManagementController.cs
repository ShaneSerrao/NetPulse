using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Data;
using PulsNet.Web.Models;
using PulsNet.Web.Services;

namespace PulsNet.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class DeviceManagementController : Controller
    {
        private readonly AppDbContext _db;
        private readonly DeviceManagementService _svc;

        public DeviceManagementController(AppDbContext db, DeviceManagementService svc)
        {
            _db = db; _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Devices = await _db.Devices.Include(d=>d.Tenant).OrderBy(d=>d.ClientName).ToListAsync();
            ViewBag.Tenants = await _db.Tenants.OrderBy(t=>t.Name).ToListAsync();
            ViewBag.Templates = await _db.Set<ConfigTemplate>().OrderBy(t=>t.Name).ToListAsync();
            ViewBag.Scripts = await _db.Set<ScriptItem>().OrderBy(s=>s.Name).ToListAsync();
            ViewBag.Firmwares = await _db.Set<FirmwareCatalog>().OrderBy(f=>f.Version).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyTemplate([FromForm] int[] deviceIds, int templateId)
        {
            var uid = User?.Identity?.Name ?? "unknown";
            var res = await _svc.ApplyTemplate(deviceIds, templateId, uid, HttpContext.RequestAborted);
            TempData["Msg"] = res.message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunScript([FromForm] int[] deviceIds, int scriptId)
        {
            var uid = User?.Identity?.Name ?? "unknown";
            var res = await _svc.RunScript(deviceIds, scriptId, uid, HttpContext.RequestAborted);
            TempData["Msg"] = res.message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FirmwareUpdate([FromForm] int[] deviceIds, string version)
        {
            var uid = User?.Identity?.Name ?? "unknown";
            var res = await _svc.FirmwareUpdate(deviceIds, version, uid, HttpContext.RequestAborted);
            TempData["Msg"] = res.message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInterface([FromForm] int[] deviceIds, string changesJson)
        {
            var uid = User?.Identity?.Name ?? "unknown";
            var res = await _svc.UpdateInterface(deviceIds, changesJson, uid, HttpContext.RequestAborted);
            TempData["Msg"] = res.message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rollback(int deviceId, int actionId)
        {
            var uid = User?.Identity?.Name ?? "unknown";
            var res = await _svc.Rollback(deviceId, actionId, uid, HttpContext.RequestAborted);
            TempData["Msg"] = res.message;
            return RedirectToAction(nameof(Index));
        }

        // Simple CRUDs for templates/scripts/firmware
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTemplate(string name, string? description, string content, int? tenantId)
        {
            _db.Add(new ConfigTemplate{ Name = name, Description = description, Content = content, TenantId = tenantId });
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateScript(string name, string? description, string content)
        {
            _db.Add(new ScriptItem{ Name = name, Description = description, Content = content });
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFirmware(string version, string? url, string? notes)
        {
            _db.Add(new FirmwareCatalog{ Version = version, Url = url, Notes = notes });
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> History(int deviceId)
        {
            var items = await _db.Set<ConfigHistory>().Where(h=>h.DeviceId==deviceId).OrderByDescending(h=>h.Timestamp).ToListAsync();
            return Json(items);
        }
    }
}