using Microsoft.AspNetCore.Mvc;

namespace PulsNet.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Dashboard");
        public IActionResult Privacy() => RedirectToAction("Index", "Dashboard");
    }
}
