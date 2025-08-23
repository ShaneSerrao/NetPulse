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
	}
}
