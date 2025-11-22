using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Sany3y.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Customer()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.UserId = userId;
            ViewBag.ApiUrl = "https://localhost:7178"; 
            return View();
        }

        public IActionResult Worker()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.UserId = userId;
            ViewBag.ApiUrl = "https://localhost:7178";
            return View();
        }

        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.UserId = userId;
            ViewBag.ApiUrl = "https://localhost:7178";
            return View();
        }
    }
}
