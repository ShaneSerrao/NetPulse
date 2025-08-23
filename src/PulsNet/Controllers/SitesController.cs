using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using PulsNet.Data;
namespace PulsNet.Controllers {
	[ApiController][Route("api/[controller]")][Authorize(Roles="Admin,SuperAdmin")]
	public sealed class SitesController : ControllerBase {
		private readonly Db _db; public SitesController(Db db){ _db=db; }
		[HttpGet] public async Task<IActionResult> All()=> Ok(await _db.Many("SELECT id,name,address,latitude,longitude FROM sites ORDER BY name", r=> new{ id=r.GetInt32(0), name=r.GetString(1), address=r.IsDBNull(2)?null:r.GetString(2), lat=r.IsDBNull(3)?(double?)null:r.GetDouble(3), lon=r.IsDBNull(4)?(double?)null:r.GetDouble(4)}));
		public sealed class Upd{ public string Name{get;set;}=""!; public string? Address{get;set;} public double? Latitude{get;set;} public double? Longitude{get;set;} }
		[HttpPost] public async Task<IActionResult> Create([FromBody] Upd b){ var id= await _db.One("INSERT INTO sites(name,address,latitude,longitude) VALUES(@n,@a,@lat,@lon) RETURNING id", r=> (int?)r.GetInt32(0), new{ n=b.Name,a=b.Address,lat=b.Latitude,lon=b.Longitude}) ?? 0; return Ok(new{ id}); }
		[HttpPut("{id}")] public async Task<IActionResult> Update(int id,[FromBody] Upd b){ await _db.Exec("UPDATE sites SET name=@n,address=@a,latitude=@lat,longitude=@lon WHERE id=@id", new{ id,n=b.Name,a=b.Address,lat=b.Latitude,lon=b.Longitude}); return Ok(); }
		[HttpDelete("{id}")] public async Task<IActionResult> Delete(int id){ await _db.Exec("DELETE FROM sites WHERE id=@id", new{ id}); return Ok(); }
		public sealed class Assign{ public int SiteId{get;set;} public int[] DeviceIds{get;set;}=Array.Empty<int>(); }
		[HttpPost("assign")] public async Task<IActionResult> AssignDevices([FromBody] Assign b){ foreach(var d in b.DeviceIds.Distinct()) await _db.Exec("INSERT INTO device_sites(device_id,site_id) VALUES(@d,@s) ON CONFLICT DO NOTHING", new{ d, s=b.SiteId}); return Ok(); }
	}
}
