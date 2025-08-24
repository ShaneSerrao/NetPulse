using System.Security.Cryptography; using System.Security.Claims; using Microsoft.AspNetCore.Authentication; using Microsoft.AspNetCore.Authentication.Cookies; using Microsoft.AspNetCore.Http; using PulsNet.Data;
namespace PulsNet.Services {
  public sealed class AuthService {
    private readonly Db _db; private readonly IHttpContextAccessor _http;
    public AuthService(Db db, IHttpContextAccessor http){ _db=db; _http=http; }
    public async Task<(bool ok,string? reason,User? u)> Verify(string username,string password){
      var u = await _db.One("SELECT id,username,role,password_hash,password_salt,two_factor_enabled,two_factor_secret FROM users WHERE username=@username",
        r=> new User{ Id=r.GetInt32(0), Username=r.GetString(1), Role=r.GetString(2), Hash=r.GetString(3), Salt=r.GetString(4), TwoFA=r.IsDBNull(5)?false:r.GetBoolean(5), Secret=r.IsDBNull(6)?null:r.GetString(6)}, new{username});
      if(u==null) return (false,"Invalid credentials",null);
      if(!VerifyPassword(password,u.Salt,u.Hash)) return (false,"Invalid credentials",null);
      return (true,null,u);
    }
    public async Task SignInAsync(User u){
      var claims = new List<Claim>{ new(ClaimTypes.NameIdentifier,u.Id.ToString()), new(ClaimTypes.Name,u.Username), new(ClaimTypes.Role,u.Role) };
      var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
      await _http.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }
    public static (string Salt,string Hash) HashPassword(string pw){
      var salt=RandomNumberGenerator.GetBytes(16); var hash=Rfc2898DeriveBytes.Pbkdf2(pw,salt,100_000,HashAlgorithmName.SHA256,32);
      return (Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }
    public static bool VerifyPassword(string pw,string saltB64,string hashB64){
      var salt=Convert.FromBase64String(saltB64); var hash=Convert.FromBase64String(hashB64);
      var test=Rfc2898DeriveBytes.Pbkdf2(pw,salt,100_000,HashAlgorithmName.SHA256,32);
      return CryptographicOperations.FixedTimeEquals(test,hash);
    }
    public sealed class User{ public int Id{get;set;} public string Username{get;set;}=""!; public string Role{get;set;}=""!; public string Hash{get;set;}=""!; public string Salt{get;set;}=""!; public bool TwoFA{get;set;} public string? Secret{get;set;} }
  }
}
