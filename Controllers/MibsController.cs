using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Data;
using System.Collections.Generic;

namespace PulsNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class MibsController : ControllerBase
    {
        private readonly Db _db;
        public MibsController(Db db) => _db = db;

        // SNMP walk request body
        public sealed class WalkBody
        {
            public string Ip { get; set; } = "!";
            public string Community { get; set; } = "public";
            public string BaseOid { get; set; } = "1.3.6.1.2.1";
        }

        [HttpPost("walk")]
        public async Task<IActionResult> Walk([FromBody] WalkBody b)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/usr/bin/snmpwalk",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            psi.ArgumentList.Add("-v");
            psi.ArgumentList.Add("2c");
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add(b.Community);
            psi.ArgumentList.Add("-O");
            psi.ArgumentList.Add("qs");
            psi.ArgumentList.Add(b.Ip);
            psi.ArgumentList.Add(b.BaseOid);

            using var p = Process.Start(psi)!;
            var o = await p.StandardOutput.ReadToEndAsync();
            await p.WaitForExitAsync();

            // Escape 'out' keyword in anonymous object
            await _db.Exec(
                "INSERT INTO mib_walk_logs(device_id,base_oid,output) VALUES(0,@o,@out)",
                new { o = b.BaseOid, @out = o }
            );

            var lines = o.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var list = lines.Select(l => new { line = l.Trim() }).ToList();
            return Ok(new { count = list.Count, entries = list.Take(200) });
        }

        [HttpGet("catalog")]
        public async Task<IActionResult> Catalog() =>
            Ok(await _db.Many(
                "SELECT id,oid,name,category_id,description FROM mib_catalog ORDER BY name",
                r => new
                {
                    id = r.GetInt32(0),
                    oid = r.GetString(1),
                    name = r.GetString(2),
                    categoryId = r.IsDBNull(3) ? (int?)null : r.GetInt32(3),
                    description = r.IsDBNull(4) ? null : r.GetString(4)
                }));

        // MIB catalog POST body
        public sealed class AddCat
        {
            public string Oid { get; set; } = "!";
            public string Name { get; set; } = "!";
            public int? CategoryId { get; set; }
            public string? Description { get; set; }
        }

        [HttpPost("catalog")]
        public async Task<IActionResult> AddCatalog([FromBody] AddCat b)
        {
            await _db.Exec(
                "INSERT INTO mib_catalog(oid,name,category_id,description) VALUES(@o,@n,@c,@d) ON CONFLICT(oid) DO NOTHING",
                new { o = b.Oid, n = b.Name, c = b.CategoryId, d = b.Description }
            );
            return Ok();
        }

        // Device-MIB attach POST body
        public sealed class AttachRequest
        {
            public int DeviceId { get; set; }
            public int[] MibIds { get; set; } = Array.Empty<int>();
        }

        [HttpPost("attach")]
        public async Task<IActionResult> Attach([FromBody] AttachRequest b)
        {
            foreach (var id in b.MibIds.Distinct())
            {
                await _db.Exec(
                    "INSERT INTO mib_device_selections(device_id,mib_id) VALUES(@d,@m) ON CONFLICT DO NOTHING",
                    new { d = b.DeviceId, m = id }
                );
            }
            return Ok();
        }

        [HttpGet("device/{deviceId}/oids")]
        public async Task<IActionResult> DeviceOids(int deviceId)
        {
            var list = await _db.Many(
                @"SELECT c.id, c.oid, c.name, c.description
                  FROM mib_device_selections s
                  JOIN mib_catalog c ON c.id = s.mib_id
                  WHERE s.device_id=@deviceId
                  ORDER BY c.name",
                r => new {
                    id = r.GetInt32(0),
                    oid = r.GetString(1),
                    name = r.GetString(2),
                    description = r.IsDBNull(3) ? null : r.GetString(3)
                },
                new { deviceId }
            );
            return Ok(list);
        }

        [HttpGet("device/{deviceId}/values")]
        public async Task<IActionResult> DeviceOidValues(int deviceId)
        {
            // get device ip + community
            var dev = await _db.One(
                "SELECT ip_address, snmp_community FROM devices WHERE id=@id",
                r => new { ip = r.GetString(0), comm = r.GetString(1) },
                new { id = deviceId });
            if (dev == null) return NotFound();

            var oids = await _db.Many(
                @"SELECT c.oid, c.name
                  FROM mib_device_selections s
                  JOIN mib_catalog c ON c.id = s.mib_id
                  WHERE s.device_id=@deviceId
                  ORDER BY c.name",
                r => new { oid = r.GetString(0), name = r.GetString(1) },
                new { deviceId });

            var results = new List<object>();
            foreach (var m in oids)
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/usr/bin/snmpget",
                    RedirectStandardOutput = true, RedirectStandardError = true
                };
                psi.ArgumentList.Add("-v"); psi.ArgumentList.Add("2c");
                psi.ArgumentList.Add("-c"); psi.ArgumentList.Add(dev.comm);
                psi.ArgumentList.Add("-O"); psi.ArgumentList.Add("qv");
                psi.ArgumentList.Add(dev.ip); psi.ArgumentList.Add(m.oid);
                try
                {
                    using var p = System.Diagnostics.Process.Start(psi)!;
                    var outp = await p.StandardOutput.ReadToEndAsync();
                    await p.WaitForExitAsync();
                    results.Add(new { m.name, m.oid, value = outp.Trim() });
                }
                catch
                {
                    results.Add(new { m.name, m.oid, value = "" });
                }
            }
            return Ok(results);
        }
    }
}
