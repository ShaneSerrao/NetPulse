using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using PulsNet.Data; using PulsNet.Services;
namespace PulsNet.Controllers {
	[ApiController][Route("api/[controller]")][Authorize(Roles="SuperAdmin")]
	public sealed class UsersController : ControllerBase {
		private readonly Db _db; public UsersController(Db db){ _db=db; }
		[HttpGet] public async Task<IActionResult> All()=> Ok(await _db.Many("SELECT id,username,role,email FROM users ORDER BY username", r=> new{ id=r.GetInt32(0), username=r.GetString(1), role=r.GetString(2), email=r.IsDBNull(3)?null:r.GetString(3)}));
		public sealed class Upd{ public string Username{get;set;}=""!; public string Password{get;set;}=""!; public string Role{get;set;}="User"; public string? Email{get;set;} }
		[HttpPost] public async Task<IActionResult> Create([FromBody] Upd b){ var (salt,hash)=AuthService.HashPassword(b.Password); await _db.Exec("INSERT INTO users(username,role,password_hash,password_salt,email) VALUES(@u,@r,@h,@s,@e)", new{ u=b.Username, r=b.Role, h=hash, s=salt, e=b.Email}); return Ok(); }
		public sealed class RoleBody{ public string Role{get;set;}="User";}
		[HttpPost("{id}/role")] public async Task<IActionResult> SetRole(int id,[FromBody] RoleBody b){ await _db.Exec("UPDATE users SET role=@r WHERE id=@id", new{ id, r=b.Role}); return Ok(); }
		public sealed class Reset{ public string Password{get;set;}=""!; }
		[HttpPost("{id}/reset")] public async Task<IActionResult> ResetPw(int id,[FromBody] Reset b){ var (s,h)=AuthService.HashPassword(b.Password); await _db.Exec("UPDATE users SET password_hash=@h,password_salt=@s WHERE id=@id", new{ id, h, s}); return Ok(); }
	}
}
