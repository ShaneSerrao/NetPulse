using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.config;
using PulsNet.Web.Data;
using PulsNet.Web.Models;
using PulsNet.Web.Services;
using PulsNet.Web.Services.Snmp;

var builder = WebApplication.CreateBuilder(args);

var secrets = SecretsLoader.Load();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(SecretsLoader.BuildPostgresConnectionString(secrets)));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
});

builder.Services.AddMemoryCache();

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<SnmpClient>();
builder.Services.AddHostedService<MonitoringService>();

// make global poll interval available via IConfiguration
builder.Configuration["GlobalPollIntervalSeconds"] = secrets.GlobalPollIntervalSeconds.ToString();

var app = builder.Build();

await SeedData.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
