using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PulsNet.Web.Models;

namespace PulsNet.Web.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Create tables if they do not exist yet
            await db.Database.EnsureCreatedAsync();
            // Optional: if you maintain migrations, you can switch to MigrateAsync()
            // await db.Database.MigrateAsync();

            foreach (var role in new[] { "SuperAdmin", "Admin", "Operator", "Viewer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var settings = await db.AppSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new AppSettings { GlobalTwoFactorEnabled = false, GlobalPollIntervalSeconds = 5, Theme = "dark" };
                db.AppSettings.Add(settings);
                await db.SaveChangesAsync();
            }

            var adminEmail = "admin@pulsnet.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, TwoFactorEnabled = false, TwoFactorPreference = false };
                await userManager.CreateAsync(admin, "PulsNet#2025");
                await userManager.AddToRoleAsync(admin, "SuperAdmin");
            }
        }
    }
}