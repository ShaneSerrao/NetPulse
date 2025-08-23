using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using PulsNet.Data;
namespace PulsNet.Controllers {
	[ApiController][Route("api/[controller]")][Authorize(Roles="Admin,SuperAdmin")]
	public sealed class TemplatesController : ControllerBase {
		private readonly Db _db; public TemplatesController(Db db){ _db=db; }
		[HttpGet] public async Task<IActionResult> All()=> Ok(await _db.Many("SELECT \"Id\",\"Name\",\"Version\",\"TenantId\" FROM \"ConfigTemplates\" ORDER BY \"Name\"", r=> new{ id=r.GetInt32(0), name=r.GetString(1), version=r.GetInt32(2), tenantId=r.IsDBNull(3)?(int?)null:r.GetInt32(3)}));
		public sealed class Upd{ public string Name{get;set;}=""!; public string Content{get;set;}=""!; public int? TenantId{get;set;} }
		[HttpPost] public async Task<IActionResult> Create([FromBody]Upd b){
			var id = await _db.One("INSERT INTO \"ConfigTemplates\"(\"Name\",\"Content\",\"TenantId\") VALUES(@n,@c,@t) RETURNING \"Id\"", r=> (int?)r.GetInt32(0), new{ n=b.Name, c=b.Content, t=b.TenantId}) ?? 0;
			return Ok(new{ id });
		}
		[HttpPut("{id}")] public async Task<IActionResult> Update(int id,[FromBody]Upd b){
			await _db.Exec("UPDATE \"ConfigTemplates\" SET \"Name\"=@n, \"Content\"=@c, \"TenantId\"=@t, \"Version\"=\"Version\"+1 WHERE \"Id\"=@id", new{ id, n=b.Name, c=b.Content, t=b.TenantId});
			return Ok();
		}
		[HttpDelete("{id}")] public async Task<IActionResult> Delete(int id){ await _db.Exec("DELETE FROM \"ConfigTemplates\" WHERE \"Id\"=@id", new{ id}); return Ok(); }
	}
}
