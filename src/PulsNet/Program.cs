using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using System.Threading.RateLimiting;
using PulsNet.Services;
using PulsNet.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Authentication & cookie configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(o =>
  {
      o.LoginPath = "/login.html";
      o.Cookie.Name = "PulsNet.Auth";
      o.Cookie.HttpOnly = true;
      o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // dev fix: allows HTTP login
      o.Cookie.SameSite = SameSiteMode.Lax;
  });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(o => {
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString() ?? "x",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddSingleton<ConfigService>();
builder.Services.AddSingleton<Db>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<DeviceService>();
builder.Services.AddSingleton<MonitoringService>();
builder.Services.AddSingleton<SettingsService>();
builder.Services.AddSingleton<DeviceManagementService>();
builder.Services.AddHostedService<ActionProcessorService>();

var app = builder.Build();
var cfg = app.Services.GetRequiredService<ConfigService>();
if (cfg.Config.Security.HttpsOnly) { app.UseHsts(); app.UseHttpsRedirection(); }

// Security headers
app.Use(async (ctx, next) => {
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

// Rate limiter
app.UseRateLimiter();

// Serve index.html at "/" by default
var defaultFiles = new DefaultFilesOptions();
defaultFiles.DefaultFileNames.Clear();
defaultFiles.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFiles);
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Seed default admin if no users exist (one-time)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PulsNet.Data.Db>();
    var auth = scope.ServiceProvider.GetRequiredService<PulsNet.Services.AuthService>();

    var count = await db.One("SELECT COUNT(1) FROM users", r => (long?)r.GetInt64(0)) ?? 0L;
    if (count == 0L)
    {
        var (salt, hash) = PulsNet.Services.AuthService.HashPassword("admin123");
        await db.Exec(
            "INSERT INTO users (username, role, password_hash, password_salt, email) " +
            "VALUES ('admin','SuperAdmin',@h,@s,'admin@example.com')",
            new { h = hash, s = salt }
        );
        Console.WriteLine("Default admin user created: username='admin', password='admin123'");
    }
}

app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.Run();

namespace PulsNet { public class Marker { } }
