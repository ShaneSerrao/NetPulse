using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Data;

namespace PulsNet.Controllers {
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public sealed class LayoutController : ControllerBase {
		private readonly Db _db;
		public LayoutController(Db db){ _db=db; }

		private int UserId() {
			var s = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			return int.TryParse(s, out var v) ? v : 0;
		}

		[HttpGet("cards")]
		public async Task<IActionResult> GetCards() {
			var key = $"layout:cards:{UserId()}";
			var json = await _db.One("SELECT value_json FROM settings WHERE key=@k", r => r.GetString(0), new { k = key });
			if (string.IsNullOrWhiteSpace(json)) return Ok(new { ids = Array.Empty<int>() });
			try { var ids = JsonSerializer.Deserialize<int[]>(json!) ?? Array.Empty<int>(); return Ok(new { ids }); } catch { return Ok(new { ids = Array.Empty<int>() }); }
		}

		public sealed class SaveBody { public int[] Ids { get; set; } = Array.Empty<int>(); }

		[HttpPost("cards")]
		public async Task<IActionResult> SaveCards([FromBody] SaveBody b) {
			var key = $"layout:cards:{UserId()}";
			var json = JsonSerializer.Serialize(b.Ids);
			await _db.Exec("INSERT INTO settings(key,value_json) VALUES(@k,@v) ON CONFLICT(key) DO UPDATE SET value_json=EXCLUDED.value_json", new { k = key, v = json });
			return Ok();
		}

		// --- Advanced layout versions ---
		public sealed class LayoutVersion { public string Name { get; set; } = "default"; public string Code { get; set; } = ""; }

		[HttpGet("versions")]
		public async Task<IActionResult> GetVersions(){
			var key = $"layout:versions:{UserId()}";
			var json = await _db.One("SELECT value_json FROM settings WHERE key=@k", r=> r.GetString(0), new{ k=key});
			if(string.IsNullOrWhiteSpace(json)) return Ok(new[]{ new LayoutVersion{ Name="Default", Code="" }});
			try{ var list = JsonSerializer.Deserialize<List<LayoutVersion>>(json!) ?? new(); if(list.Count==0) list.Add(new LayoutVersion{ Name="Default", Code=""}); return Ok(list);}catch{ return Ok(new[]{ new LayoutVersion{ Name="Default", Code="" }}); }
		}

		public sealed class SaveVersionBody { public string Name { get; set; } = ""; public string Code { get; set; } = ""; }
		[HttpPost("versions")] public async Task<IActionResult> SaveVersion([FromBody] SaveVersionBody b){
			var key = $"layout:versions:{UserId()}";
			var json = await _db.One("SELECT value_json FROM settings WHERE key=@k", r=> r.GetString(0), new{ k=key});
			var list = string.IsNullOrWhiteSpace(json)? new List<LayoutVersion>() : (JsonSerializer.Deserialize<List<LayoutVersion>>(json!) ?? new());
			var existing = list.FirstOrDefault(x=> string.Equals(x.Name, b.Name, StringComparison.OrdinalIgnoreCase));
			if(existing==null) list.Add(new LayoutVersion{ Name=b.Name, Code=b.Code}); else existing.Code=b.Code;
			var outp = JsonSerializer.Serialize(list);
			await _db.Exec("INSERT INTO settings(key,value_json) VALUES(@k,@v) ON CONFLICT(key) DO UPDATE SET value_json=EXCLUDED.value_json", new{ k=key, v=outp});
			return Ok();
		}

		public sealed class DeleteVersionBody { public string Name { get; set; } = ""; }
		[HttpPost("versions/delete")] public async Task<IActionResult> DeleteVersion([FromBody] DeleteVersionBody b){
			if(string.Equals(b.Name, "Default", StringComparison.OrdinalIgnoreCase)) return BadRequest("Cannot delete Default");
			var key = $"layout:versions:{UserId()}";
			var json = await _db.One("SELECT value_json FROM settings WHERE key=@k", r=> r.GetString(0), new{ k=key});
			var list = string.IsNullOrWhiteSpace(json)? new List<LayoutVersion>() : (JsonSerializer.Deserialize<List<LayoutVersion>>(json!) ?? new());
			list = list.Where(x=> !string.Equals(x.Name, b.Name, StringComparison.OrdinalIgnoreCase)).ToList();
			var outp = JsonSerializer.Serialize(list);
			await _db.Exec("INSERT INTO settings(key,value_json) VALUES(@k,@v) ON CONFLICT(key) DO UPDATE SET value_json=EXCLUDED.value_json", new{ k=key, v=outp});
			return Ok();
		}
	}
}
