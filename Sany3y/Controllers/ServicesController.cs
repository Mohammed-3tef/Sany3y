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
            string? name,
            decimal? minPrice,
            decimal? maxPrice,
            double? rating)
        {
            var users = await _http.GetFromJsonAsync<List<User>>("api/Technician/GetAll");

            if (categoryId != null)
                users = users.Where(u => u.CategoryID == categoryId).ToList();

            if (!string.IsNullOrEmpty(city))
                users = users.Where(u => u.Address != null && u.Address.City.Contains(city)).ToList();

            if (!string.IsNullOrEmpty(name))
                users = users.Where(u => (u.FirstName + " " + u.LastName).Contains(name) || u.FirstName.Contains(name) || u.LastName.Contains(name)).ToList();

            if (minPrice.HasValue)
                users = users.Where(u => u.Price >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                users = users.Where(u => u.Price <= maxPrice.Value).ToList();

            if (rating != null)
                users = users.Where(u => u.Rating >= rating.Value).ToList();

            return View(users);


        }

        //--------------------------------------

        public static string NormalizeArabic(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string normalized = text;

            normalized = normalized.Replace("أ", "ا")
                                   .Replace("إ", "ا")
                                   .Replace("آ", "ا")
                                   .Replace("ة", "ه")
                                   .Replace("ى", "ي")
                                   .Replace("ؤ", "و")
                                   .Replace("ئ", "ي");

            string diacritics = @"[\u064B-\u0652]";
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, diacritics, "");

            return normalized;
        }

        //--------------- البحث من الهوم -----------------
        public async Task<IActionResult> Search(string serviceType)
        {
            var users = await _http.GetFromJsonAsync<List<User>>("api/Technician/GetAll");

            if (!string.IsNullOrEmpty(serviceType))
            {
                // Normalize input
                serviceType = NormalizeArabic(serviceType);

                int? categoryId = serviceType switch
                {
                    "بناء وتشييد" => 1,
                    "كهربا" => 2,
                    "سباكه" => 3,
                    "دهانات و تشطيبات" => 4,
                    "نجاره" => 5,
                    "حداده" => 6,
                    "الوميتال" => 7,
                    "مقاولات عامه" => 8,
                    "رخام وسيراميك" => 9,
                    "نقاشه" => 10,
                    "تكييف وتبريد" => 11,
                    "صيانه اجهزه" => 12,
                    "تركيبات" => 13,
                    "تنظيف وتجهيز" => 14,
                    "نقل عفش وخدمات لوجستيه" => 15,
                    "خدمات اخرى" => 16,
                    _ => null
                };

                if (categoryId != null)
                {
                    users = users.Where(u => u.CategoryID == categoryId).ToList();
                }
            }

            return View("Index", users);
        }

        // -------------------------------------------------------------
        // 🔵 صفحة تفاصيل الفني
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

            // --------------------------------------------------------------------
            // إضافة الـ CurrentUserId من الـ Login الحقيقي
            // --------------------------------------------------------------------
            ViewBag.CurrentUserId = User.Identity.IsAuthenticated
                ? long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value)
                : 0;

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedule(int technicianId)
        {
            var schedule = await _http.GetFromJsonAsync<List<TechnicianSchedule>>($"/api/TechnicianSchedule/GetSchedule/{technicianId}");
            return Json(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> BookSlot([FromBody] BookingRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/TechnicianSchedule/BookSlot", request);
            if (response.IsSuccessStatusCode)
            {
                return Ok("Booked successfully");
            }
            return BadRequest("Booking failed");
        }

        public class BookingRequest
        {
            public int ScheduleId { get; set; }
            public int CustomerId { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetConversation(long otherUserId)
        {
            var currentUserId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var messages = await _http.GetFromJsonAsync<List<Message>>($"/api/Message/GetConversation/{currentUserId}/{otherUserId}");
            return Json(messages);
        }
    }
}
