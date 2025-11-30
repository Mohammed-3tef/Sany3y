using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using System.Security.Claims;

namespace Sany3y.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public async Task<IActionResult> CustomerAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            long userId = long.Parse(userIdStr);

            ViewBag.UserId = userId;
            ViewBag.ApiUrl = "https://localhost:7178";

            List<User> chatPartners = new List<User>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ViewBag.ApiUrl);

                // جلب كل الأشخاص اللي اتكلم معهم المستخدم مباشرة من الـ API
                var response = await client.GetAsync($"/api/Message/GetChatPartners/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    chatPartners = await response.Content.ReadFromJsonAsync<List<User>>();
                }
            }

            ViewBag.ChatPartners = chatPartners;
            return View();
        }

        public async Task<IActionResult> Worker()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            long userId = long.Parse(userIdStr);

            ViewBag.UserId = userId;
            ViewBag.ApiUrl = "https://localhost:7178";

            List<User> chatPartners = new List<User>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ViewBag.ApiUrl);

                // جلب كل الأشخاص اللي اتكلم معهم المستخدم مباشرة من الـ API
                var response = await client.GetAsync($"/api/Message/GetChatPartners/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    chatPartners = await response.Content.ReadFromJsonAsync<List<User>>();
                }
            }

            ViewBag.ChatPartners = chatPartners;
            return View();
        }
    }
}
