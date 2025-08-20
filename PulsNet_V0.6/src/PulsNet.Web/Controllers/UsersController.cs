using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Web.Models;

namespace PulsNet.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create() => View(new ApplicationUser());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string password, string role = "Viewer")
        {
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, TwoFactorEnabled = false };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(user);
            }
            if (!await _roleManager.RoleExistsAsync(role)) await _roleManager.CreateAsync(new IdentityRole(role));
            await _userManager.AddToRoleAsync(user, role);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id); if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string? email, bool? twoFactor)
        {
            var user = await _userManager.FindByIdAsync(id); if (user == null) return NotFound();
            if (!string.IsNullOrWhiteSpace(email)) { user.Email = email; user.UserName = email; }
            if (twoFactor.HasValue) user.TwoFactorEnabled = twoFactor.Value;
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id); if (user != null) await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}