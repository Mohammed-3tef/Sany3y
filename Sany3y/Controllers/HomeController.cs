using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.ViewModels; // لو حطينا الـ HomeIndexViewModel هنا

namespace Sany3y.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _http;

        public HomeController(HttpClient http, IHttpClientFactory httpClientFactory)
        {
            _http = http;
            _http = httpClientFactory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7178/");
        }

        public async Task<IActionResult> Index()
        {
            // جلب أول 8 كاتيجوريز من قاعدة البيانات
            var response = await _http.GetAsync("/api/Category/GetAll");
            if (response == null)
                return View();
            
            var categories = await response.Content.ReadFromJsonAsync<List<Category>>();

            var model = new HomeIndexViewModel
            {
                Categories = categories.Take(8).ToList()
            };
            return View(model);
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

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}