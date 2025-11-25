using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;


namespace Sany3y.Controllers
{
    public class ServicesController : Controller
    {
        private readonly HttpClient _http;

        public ServicesController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7178/");
        }

        public async Task<IActionResult> Index(
     int? categoryId,
     string? city,
     decimal? minPrice,
     decimal? maxPrice,
     double? rating)

        {
            // Get all technicians
            var users = await _http.GetFromJsonAsync<List<User>>("api/Technician/GetAll");

            // Filtering
            if (categoryId != null)
                users = users.Where(u => u.CategoryID == categoryId).ToList();

            if (!string.IsNullOrEmpty(city))
                users = users.Where(u => u.Address != null && u.Address.City.Contains(city)).ToList();

            if (minPrice.HasValue)
                users = users.Where(u => u.Price >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                users = users.Where(u => u.Price <= maxPrice.Value).ToList();

            if (rating != null)
                users = users.Where(u => u.Rating >= rating.Value).ToList();



            return View(users);
        }


        public async Task<IActionResult> Search(string serviceType)
        {
            var users = await _http.GetFromJsonAsync<List<User>>("api/Technician/GetAll");
            return View("Index", users);
        }


        // -------------------------------------------------------------
        // 🔵 NEW: Service Details Page
        // -------------------------------------------------------------
        public async Task<IActionResult> Details(int id)
        {
            var user = await _http.GetFromJsonAsync<User>($"api/Technician/GetAll/{id}");

            if (user == null)
                return NotFound();

            return View(user);
        }
    }
}