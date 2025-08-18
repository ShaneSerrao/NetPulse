using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Data;
using PulsNet.Services;

namespace PulsNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public sealed class AdminController : ControllerBase
    {
        private readonly Db _db;
        private readonly SettingsService _settings;

        public AdminController(Db db, SettingsService settings)
        {
            _db = db;
            _settings = settings;
        }

        [HttpPost("user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req, CancellationToken ct)
        {
            var (salt, hash) = AuthService.HashPassword(req.Password);
            await _db.ExecuteAsync("INSERT INTO users (username, role, password_hash, password_salt, email) VALUES (@u, @r, @h, @s, @e)", new { u = req.Username, r = req.Role, h = hash, s = salt, e = req.Email }, ct);
            return Ok();
        }

        [HttpGet("settings/theme")]
        public async Task<IActionResult> Theme(CancellationToken ct)
        {
            var theme = await _settings.GetThemeAsync(ct);
            return Ok(theme);
        }

        public sealed class CreateUserRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = "User";
            public string? Email { get; set; }
        }
    }
}

