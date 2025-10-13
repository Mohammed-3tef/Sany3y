using Microsoft.AspNetCore.Mvc;

namespace Sany3y.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
