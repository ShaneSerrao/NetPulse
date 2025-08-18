using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PulsNet.Services;

namespace PulsNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly AuthService _auth;
        private readonly SettingsService _settings;

        public AuthController(AuthService auth, SettingsService settings)
        {
            _auth = auth;
            _settings = settings;
        }

        public sealed class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string? TotpCode { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var global2fa = await _settings.GetGlobal2FAEnabledAsync(ct);
            var (ok, reason, user) = await _auth.AuthenticateAsync(request.Username, request.Password, request.TotpCode, ct);
            if (!ok || user == null) return Unauthorized(new { error = reason ?? "Unauthorized" });

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Ok(new { username = user.Username, role = user.Role, twoFactor = user.TwoFactorEnabled || global2fa });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new { username = User.Identity?.Name, role = User.FindFirstValue(ClaimTypes.Role) });
        }

        [Authorize]
        [HttpPost("enable-2fa")]
        public async Task<IActionResult> Enable2FA(CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return BadRequest();
            var secret = AuthService.GenerateTotpSecret();
            await _auth.Enable2FAAsync(userId, secret, ct);
            var otpauth = $"otpauth://totp/PulsNet:{User.Identity?.Name}?secret={secret}&issuer=PulsNet";
            return Ok(new { secret, otpauth });
        }
    }
}

