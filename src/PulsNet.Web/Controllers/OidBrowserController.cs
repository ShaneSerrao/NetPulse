using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Data;
using PulsNet.Web.Models;
using PulsNet.Web.Services.Snmp;
using System.Text.RegularExpressions;

namespace PulsNet.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Operator")]
    public class OidBrowserController : Controller
    {
        private readonly AppDbContext _db;
        private readonly SnmpClient _snmp;
        public OidBrowserController(AppDbContext db, SnmpClient snmp){ _db = db; _snmp = snmp; }

        [HttpGet]
        public async Task<IActionResult> Index(int deviceId)
        {
            var d = await _db.Devices.FindAsync(deviceId);
            if (d == null) return NotFound();
            ViewBag.Device = d;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Walk(int deviceId, string baseOid = "1.3.6.1.2.1")
        {
            var d = await _db.Devices.FindAsync(deviceId);
            if (d == null) return NotFound();
            var rows = await _snmp.WalkAsync(d.IpAddress, d.SnmpCommunity, baseOid, d.SnmpPort);
            var categories = Categorize(rows);
            return Json(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSelected(int deviceId, [FromForm] string[] selectedOids, [FromForm] string[] labels, [FromForm] string[] categories)
        {
            var d = await _db.Devices.FindAsync(deviceId);
            if (d == null) return NotFound();
            for (int i=0;i<selectedOids.Length;i++)
            {
                var oid = selectedOids[i];
                var label = i<labels.Length ? labels[i] : null;
                var cat = i<categories.Length ? categories[i] : null;
                if (string.IsNullOrWhiteSpace(oid)) continue;
                if (!await _db.DeviceSelectedOids.AnyAsync(x=>x.DeviceId==deviceId && x.Oid==oid))
                {
                    _db.DeviceSelectedOids.Add(new DeviceSelectedOid{ DeviceId = deviceId, Oid = oid, Label = label, Category = cat });
                }
            }
            await _db.SaveChangesAsync();
            TempData["Msg"] = "Selected OIDs saved";
            return RedirectToAction(nameof(Index), new { deviceId });
        }

        private static Dictionary<string,List<(string oid,string name,string value)>> Categorize(List<(string oid,string value)> rows)
        {
            var r = new Dictionary<string,List<(string,string,string)>>();
            void add(string cat, (string oid,string value) x, string name){
                if(!r.ContainsKey(cat)) r[cat] = new List<(string,string,string)>();
                r[cat].Add((x.oid,name,x.value));
            }
            var ifDescr = new Regex("IF-MIB::ifDescr\\.(\\d+)");
            var ifInOct = new Regex("IF-MIB::ifInOctets\\.(\\d+)");
            var ifOutOct = new Regex("IF-MIB::ifOutOctets\\.(\\d+)");
            foreach (var x in rows)
            {
                if (x.oid.StartsWith("IF-MIB::ifDescr")) add("Interfaces", x, x.value.Replace("STRING:", string.Empty).Trim());
                else if (x.oid.StartsWith("IF-MIB::ifInOctets") || x.oid.StartsWith("IF-MIB::ifOutOctets") || x.oid.StartsWith("IF-MIB::ifHCInOctets") || x.oid.StartsWith("IF-MIB::ifHCOutOctets")) add("Traffic", x, x.oid);
                else if (x.oid.StartsWith("SNMPv2-MIB::sys")) add("System", x, x.oid);
                else if (x.oid.StartsWith("IP-MIB::")) add("IP", x, x.oid);
                else add("Other", x, x.oid);
            }
            return r;
        }
    }
}