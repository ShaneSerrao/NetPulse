using Microsoft.AspNetCore.Identity;
using PulsNet.Web.Data;
using PulsNet.Web.Models;

namespace PulsNet.Web.Middleware
{
    public class IdleTimeoutMiddleware
    {
        private readonly RequestDelegate _next;
        public IdleTimeoutMiddleware(RequestDelegate next){_next = next;}

        public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager, AppDbContext db)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    var settings = await db.AppSettings.FindAsync(1) ?? new AppSettings();
                    var timeout = user.IdleTimeoutMinutes ?? settings.DefaultIdleTimeoutMinutes;
                    var last = context.Session.GetString("last-activity");
                    var nowTicks = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (last != null && long.TryParse(last, out var prev))
                    {
                        if ((nowTicks - prev) > timeout * 60)
                        {
                            // expire
                            context.Response.Redirect("/Account/Login");
                            return;
                        }
                    }
                    context.Session.SetString("last-activity", nowTicks.ToString());
                }
            }
            await _next(context);
        }
    }

    public static class IdleTimeoutMiddlewareExtensions
    {
        public static IApplicationBuilder UseIdleTimeout(this IApplicationBuilder app)
        {
            return app.UseMiddleware<IdleTimeoutMiddleware>();
        }
    }
}