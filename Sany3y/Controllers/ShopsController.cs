using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using System.Net.Http;
using Task = System.Threading.Tasks.Task;

namespace Sany3y.Controllers
{
    public class ShopsController : Controller
    {
        private readonly HttpClient _http;

        public ShopsController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7178/"); // Ensure this matches your API URL
        }

        public async Task<IActionResult> Index(
            int? categoryId,
            string? governorate,
            string? city,
            string? name)
        {
            await PopulateFilters();

            // 1. Get ALL technicians (since Shops are currently a subset of technicians/users)
            // Ideally should be api/User/GetShops but api/Technician/GetAll gives us users to filter.
            var users = await _http.GetFromJsonAsync<List<User>>("api/Technician/GetAll");

            // 2. Filter for Shops ONLY
            if (users != null)
            {
                // Filter by IsShop == true
                // Note: IsShop is nullable bool?
                users = users.Where(u => u.IsShop != null && u.IsShop == true).ToList();

                // 3. Apply other filters
                if (categoryId != null)
                    users = users.Where(u => u.CategoryID == categoryId).ToList();

                if (!string.IsNullOrEmpty(governorate))
                    users = users.Where(u => u.Address != null && u.Address.Governorate.Contains(governorate)).ToList();

                if (!string.IsNullOrEmpty(city))
                    users = users.Where(u => u.Address != null && u.Address.City.Contains(city)).ToList();

                if (!string.IsNullOrEmpty(name))
                    users = users.Where(u =>
                        (u.ShopName != null && u.ShopName.Contains(name)) ||
                        (u.FirstName + " " + u.LastName).Contains(name)
                    ).ToList();
            }
            else
            {
                users = new List<User>();
            }

            return View(users);
        }

        private async Task PopulateFilters()
        {
            try
            {
                var categories = await _http.GetFromJsonAsync<List<Category>>("api/Category/GetAll");
                var governorates = await _http.GetFromJsonAsync<List<Governorate>>("api/CountryServices/GetAllGovernorates");
                ViewBag.Categories = categories;
                ViewBag.Governorates = governorates;
            }
            catch
            {
                // Handle case where API is down or errors
                ViewBag.Categories = new List<Category>();
                ViewBag.Governorates = new List<Governorate>();
            }
        }

        // -------------------------------------------------------------
        // 🔵 صفحة تفاصيل المحل
        // -------------------------------------------------------------
        public async Task<IActionResult> Details(int id)
        {
            var user = await _http.GetFromJsonAsync<User>($"/api/Technician/GetByID/{id}");

            if (user == null)
                return NotFound();

            var userAddress = await _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{user.AddressId}");
            var userCategory = await _http.GetFromJsonAsync<Category>($"/api/Category/GetByID/{user.CategoryID}");
            var userRatings = await _http.GetFromJsonAsync<List<Rating>>($"/api/Rating/GetByTaskerId/{id}");

            ViewBag.UserAddress = userAddress;
            ViewBag.UserCategory = userCategory;
            ViewBag.UserRatings = userRatings;

            var pictureResponse = await _http.GetAsync($"/api/ProfilePicture/GetByID/{user.ProfilePictureId}");
            if (pictureResponse.IsSuccessStatusCode)
            {
                var pictureData = await pictureResponse.Content.ReadFromJsonAsync<ProfilePicture>();
                ViewBag.UserProfilePicture = pictureData?.Path;
            }
            else
            {
                ViewBag.UserProfilePicture = "https://placehold.co/200x200?text=Shop";
            }

            // --------------------------------------------------------------------
            // إضافة الـ CurrentUserId من الـ Login الحقيقي
            // --------------------------------------------------------------------
            ViewBag.CurrentUserId = User.Identity.IsAuthenticated
                ? long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value)
                : 0;

            ViewBag.CurrentUserName = User.Identity.Name;
            return View(user);
        }
    }
}
