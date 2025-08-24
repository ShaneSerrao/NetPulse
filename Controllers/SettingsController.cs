using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Services;

namespace PulsNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class SettingsController : ControllerBase
    {
        private readonly SettingsService _s;
        private readonly ConfigService _cfg;
        public SettingsController(SettingsService s, ConfigService cfg) { _s = s; _cfg = cfg; }

        [Authorize]
        [HttpGet("theme")]
        public async Task<IActionResult> Theme() => Ok(await _s.GetTheme());

        public sealed class ThemeBody
        {
            public string? Name { get; set; }
            public string? Primary { get; set; }
            public string? Accent { get; set; }
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("theme")]
        public async Task<IActionResult> SaveTheme([FromBody] ThemeBody b)
        {
            var t = await _s.GetTheme();
            if (!string.IsNullOrWhiteSpace(b.Name)) t.Name = b.Name!;
            if (!string.IsNullOrWhiteSpace(b.Primary)) t.Primary = b.Primary!;
            if (!string.IsNullOrWhiteSpace(b.Accent)) t.Accent = b.Accent!;
            await _s.SaveTheme(t);
            return Ok();
        }

        [Authorize]
        [HttpGet("polling")]
        public async Task<IActionResult> GP()
        {
            var (i, c, o) = await _s.GetPolling();
            return Ok(new { globalIntervalSeconds = i, cacheSeconds = c, offlineThresholdSeconds = o });
        }

        public sealed class PollBody
        {
            public int GlobalIntervalSeconds { get; set; }
            public int CacheSeconds { get; set; }
            public int OfflineThresholdSeconds { get; set; }
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("polling")]
        public async Task<IActionResult> SP([FromBody] PollBody b)
        {
            await _s.SavePolling(b.GlobalIntervalSeconds, b.CacheSeconds, b.OfflineThresholdSeconds);
            return Ok();
        }

        // --- 2FA endpoints ---
        public sealed class Toggle { public bool Enabled { get; set; } }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("global2fa")]
        public async Task<IActionResult> Get2FA([FromServices] PulsNet.Data.Db db)
        {
            var val = await db.One("SELECT value_json FROM settings WHERE key='global2fa'", r => r.GetString(0));
            bool enabled = false; // default to disabled
            if (!string.IsNullOrWhiteSpace(val) && bool.TryParse(val, out var b)) enabled = b;
            return Ok(new { enabled });
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("global2fa")]
        public async Task<IActionResult> Set2FA([FromBody] Toggle b, [FromServices] PulsNet.Data.Db db)
        {
            await db.Exec(
                "INSERT INTO settings(key,value_json) VALUES('global2fa',@v) ON CONFLICT(key) DO UPDATE SET value_json=EXCLUDED.value_json",
                new { v = b.Enabled.ToString() }
            );
            return Ok();
        }
    }
}
