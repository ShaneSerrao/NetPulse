using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using PulsNet.Data;
namespace PulsNet.Controllers {
	[ApiController][Route("api/[controller]")][Authorize(Roles="Admin,SuperAdmin")]
	public sealed class TenantsController : ControllerBase {
		private readonly Db _db; public TenantsController(Db db){ _db=db; }
		[HttpGet] public async Task<IActionResult> All()=> Ok(await _db.Many("SELECT id,name FROM tenants ORDER BY name", r=> new{ id=r.GetInt32(0), name=r.GetString(1)}));
		public sealed class Upd{ public string Name{get;set;}=""!; }
		[HttpPost] public async Task<IActionResult> Create([FromBody] Upd b){ var id= await _db.One("INSERT INTO tenants(name) VALUES(@n) RETURNING id", r=> (int?)r.GetInt32(0), new{ n=b.Name}) ?? 0; return Ok(new{ id}); }
		[HttpDelete("{id}")] public async Task<IActionResult> Delete(int id){ await _db.Exec("DELETE FROM tenants WHERE id=@id", new{ id}); return Ok(); }
		public sealed class Assign{ public int TenantId{get;set;} public int[] DeviceIds{get;set;}=Array.Empty<int>(); }
		[HttpPost("assign")] public async Task<IActionResult> AssignDevices([FromBody] Assign b){ foreach(var d in b.DeviceIds.Distinct()) await _db.Exec("INSERT INTO device_tenants(device_id,tenant_id) VALUES(@d,@t) ON CONFLICT DO NOTHING", new{ d, t=b.TenantId}); return Ok(); }
	}
}
