using Microsoft.AspNetCore.Mvc;

namespace Sany3y.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Policy()  
        {
            return View();
        }

        public IActionResult OurTeam()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }
    }
}
