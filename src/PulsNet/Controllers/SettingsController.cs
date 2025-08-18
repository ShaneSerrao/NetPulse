using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Services;

namespace PulsNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class SettingsController : ControllerBase
    {
        private readonly SettingsService _settings;

        public SettingsController(SettingsService settings)
        {
            _settings = settings;
        }

        [HttpGet("theme")]
        [Authorize]
        public async Task<IActionResult> GetTheme(CancellationToken ct)
        {
            var theme = await _settings.GetThemeAsync(ct);
            return Ok(theme);
        }

        [HttpPost("theme")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> SetTheme([FromBody] ThemeConfig theme, CancellationToken ct)
        {
            await _settings.SetThemeAsync(theme, ct);
            return Ok();
        }

        [HttpGet("polling")]
        [Authorize]
        public async Task<IActionResult> GetPolling(CancellationToken ct)
        {
            var polling = await _settings.GetPollingAsync(ct);
            return Ok(polling);
        }

        [HttpPost("polling")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> SetPolling([FromBody] PollingConfig polling, CancellationToken ct)
        {
            await _settings.SetPollingAsync(polling, ct);
            return Ok();
        }

        [HttpGet("global2fa")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Get2FA(CancellationToken ct)
        {
            var enabled = await _settings.GetGlobal2FAEnabledAsync(ct);
            return Ok(new { enabled });
        }

        [HttpPost("global2fa")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Set2FA([FromBody] Toggle body, CancellationToken ct)
        {
            await _settings.SetGlobal2FAEnabledAsync(body.Enabled, ct);
            return Ok();
        }

        public sealed class Toggle { public bool Enabled { get; set; } }
    }
}

