using System.Security.Claims; using Microsoft.AspNetCore.Authentication; using Microsoft.AspNetCore.Authentication.Cookies; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using PulsNet.Services;
namespace PulsNet.Controllers {
  [ApiController][Route("api/[controller]")]
  public sealed class AuthController : ControllerBase {
    private readonly AuthService _auth;
    public AuthController(AuthService auth){ _auth=auth; }
    public sealed class LoginReq{ public string Username{get;set;}=""!; public string Password{get;set;}=""!; public string? TotpCode{get;set;} }
    [HttpPost("login")] public async Task<IActionResult> Login([FromBody] LoginReq r){
      var (ok,reason,user)= await _auth.Verify(r.Username,r.Password); if(!ok || user==null) return Unauthorized(new{error=reason??"Unauthorized"});
      await _auth.SignInAsync(user); return Ok(new{ username=user.Username, role=user.Role });
    }
    [Authorize][HttpPost("logout")] public async Task<IActionResult> Logout(){ await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); return Ok(); }
    [Authorize][HttpGet("me")] public IActionResult Me()=> Ok(new{ username=User.Identity?.Name, role=User.FindFirstValue(ClaimTypes.Role)});
  }
}
