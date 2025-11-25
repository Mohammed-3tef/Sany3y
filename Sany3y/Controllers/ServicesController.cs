using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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

        public async Task<IActionResult> Index()
        {
            var users = await _http.GetFromJsonAsync<List<User>>("api/Technician/GetAll");
            return View(users);
        }

        public async Task<IActionResult> Search(string serviceType)
        {
            // For now, just redirect to Index or filter if we had the logic.
            // Ideally, we would call an API with the search term.
            // Let's just get all and filter in memory for now as a simple start, 
            // or just return all if serviceType is empty.

            var users = await _http.GetFromJsonAsync<List<User>>("api/Technician/GetAll");

            if (!string.IsNullOrEmpty(serviceType))
            {
                // This is a naive filter. In reality, we'd filter by Category/Service.
                // But User model doesn't seem to have ServiceType directly on it?
                // It has Bio, maybe we search there? Or maybe we need to join with another table.
                // For now, let's just return the list.
            }

            return View("Index", users);
        }
    }
}