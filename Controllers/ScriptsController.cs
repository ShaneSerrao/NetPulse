using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using PulsNet.Data;
namespace PulsNet.Controllers {
	[ApiController][Route("api/[controller]")][Authorize(Roles="Admin,SuperAdmin")]
	public sealed class ScriptsController : ControllerBase {
		private readonly Db _db; public ScriptsController(Db db){ _db=db; }
		[HttpGet] public async Task<IActionResult> All()=> Ok(await _db.Many("SELECT \"Id\",\"Name\",\"Version\",\"TenantId\" FROM \"Scripts\" ORDER BY \"Name\"", r=> new{ id=r.GetInt32(0), name=r.GetString(1), version=r.GetInt32(2), tenantId=r.IsDBNull(3)?(int?)null:r.GetInt32(3)}));
		public sealed class Upd{ public string Name{get;set;}=""!; public string ScriptText{get;set;}=""!; public int? TenantId{get;set;} }
		[HttpPost] public async Task<IActionResult> Create([FromBody]Upd b){
			var id = await _db.One("INSERT INTO \"Scripts\"(\"Name\",\"ScriptText\",\"TenantId\") VALUES(@n,@s,@t) RETURNING \"Id\"", r=> (int?)r.GetInt32(0), new{ n=b.Name, s=b.ScriptText, t=b.TenantId}) ?? 0;
			return Ok(new{ id });
		}
		[HttpPut("{id}")] public async Task<IActionResult> Update(int id,[FromBody]Upd b){
			await _db.Exec("UPDATE \"Scripts\" SET \"Name\"=@n, \"ScriptText\"=@s, \"TenantId\"=@t, \"Version\"=\"Version\"+1 WHERE \"Id\"=@id", new{ id, n=b.Name, s=b.ScriptText, t=b.TenantId});
			return Ok();
		}
		[HttpDelete("{id}")] public async Task<IActionResult> Delete(int id){ await _db.Exec("DELETE FROM \"Scripts\" WHERE \"Id\"=@id", new{ id}); return Ok(); }
	}
}
