using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Data;
using PulsNet.Web.Models;
using PulsNet.Web.Services;
using PulsNet.Web.Services.Snmp;
using PulsNet.Web.Middleware;
using System;

var builder = WebApplication.CreateBuilder(args);

// Read configuration from appsettings.json
var postgresConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(postgresConnectionString))
{
    throw new InvalidOperationException("The PostgreSQL connection string is not set.");
}

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

// Add identity services
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Tokens.AuthenticatorTokenProvider = null;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
});

// Add memory cache
builder.Services.AddMemoryCache();

// Add controllers with views
builder.Services.AddControllersWithViews();

// Register singleton services
builder.Services.AddSingleton<SnmpClient>();
builder.Services.AddHostedService<MonitoringService>();
builder.Services.AddSingleton<ServerMetricsService>();
builder.Services.AddHostedService<ServerMetricsService>(sp => sp.GetRequiredService<ServerMetricsService>());

// Add scoped services
builder.Services.AddScoped<DeviceManagementService>();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed data if needed
await SeedData.InitializeAsync(app.Services);

// Production environment configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enable serving static files (necessary for serving JS, CSS, images)
app.UseStaticFiles();

// Enable routing and authentication
app.UseRouting();
app.UseSession();
app.UseIdleTimeout();
app.UseAuthentication();
app.UseAuthorization();

// Configure the default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// Run the application
app.Run();
