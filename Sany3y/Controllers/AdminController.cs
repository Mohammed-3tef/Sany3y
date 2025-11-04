using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using Sany3y.Infrastructure.Services;

namespace Sany3y.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _http;
        private readonly UserManager<User> _userManager;

        public AdminController(IHttpClientFactory httpClientFactory, UserManager<User> userManager)
        {
            _http = httpClientFactory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7178/");

            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        private async System.Threading.Tasks.Task GetTopCategories()
        {
            var allCategories = await _http.GetFromJsonAsync<List<Category>>("/api/Category/GetAll");
            var categoryCounts = new List<object>();
            var allTasks = await _http.GetFromJsonAsync<List<Infrastructure.Models.Task>>("/api/Task/GetAll");
            if (allTasks.Count <= 0 || allCategories.Count <= 0) return;
            
            foreach (var category in allCategories)
            {
                var count = allTasks.Count(t => t.CategoryId == category.Id);
                categoryCounts.Add(new { Name = category.Name, Count = count });
            }

            ViewBag.TopCategories = categoryCounts
                .OrderByDescending(c => ((dynamic)c).TaskCount).ToList();

            return;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var totalUsers = await _http.GetFromJsonAsync<List<User>>("/api/User/GetAll");
            var totalUserCount = 0;
            var totalTaskerCount = 0;

            foreach (var user in totalUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User"))
                    totalUserCount++;
                if (roles.Contains("Tasker"))
                    totalTaskerCount++;
            }

            ViewBag.TotalUsers = totalUserCount;
            ViewBag.TotalTaskers = totalTaskerCount;

            var totalCategories = await _http.GetFromJsonAsync<List<Category>>("/api/Category/GetAll");
            ViewBag.TotalCategories = totalCategories.Count;

            await GetTopCategories();

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> GetMonthlyUserCounts()
        {
            var allUsers = await _http.GetFromJsonAsync<List<User>>("/api/User/GetAll");
            var nonAdminUsers = new List<User>();

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (!roles.Contains("Admin")) nonAdminUsers.Add(u);
            }

            var currentYear = DateTime.Now.Year;
            var months = new List<string>
            {
                "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
                "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
            };

            var monthlyUserCounts = new List<object>();
            for (int month = 1; month <= 12; month++)
            {
                var count = nonAdminUsers.Count(u => u.CreatedAt.Year == currentYear && u.CreatedAt.Month == month);
                monthlyUserCounts.Add( new {
                    Month = months[month - 1],
                    UserCount = count
                });
            }

            return Json(monthlyUserCounts);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> GetGenderDistribution()
        {
            var allUsers = await _http.GetFromJsonAsync<List<User>>("/api/User/GetAll");
            var nonAdminUsers = new List<User>();
            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (!roles.Contains("Admin")) nonAdminUsers.Add(u);
            }

            var maleCount = nonAdminUsers.Count(u => u.Gender == 'M');
            var femaleCount = nonAdminUsers.Count(u => u.Gender == 'F');
            var genderDistribution = new List<object>
            {
                new { Gender = "ذكر", Count = maleCount },
                new { Gender = "أنثي", Count = femaleCount }
            };

            return Json(genderDistribution);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Users()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var currentUser = await _userManager.GetUserAsync(User);
            var users = (await _http.GetFromJsonAsync<List<User>>($"/api/User/GetAll"))?.Where(u => u.UserName != currentUser?.UserName);
            var userRoles = new List<(User User, IList<string> Roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add((user, roles));
            }

            return View(userRoles);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ViewUser(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var user = await _http.GetFromJsonAsync<User>($"/api/User/GetByID/{id}");
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Address = await _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{user.AddressId}");
            var viewModel = (User: user, Roles: roles);
            return PartialView("_ViewUserModal", viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var user = await _http.GetFromJsonAsync<User>($"/api/User/GetByID/{id}");
            if (user == null)
                return NotFound();

            await _http.GetFromJsonAsync<Address>($"/api/User/Delete/{user.Id}");
            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction("Users");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Categories()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var categories = await _http.GetFromJsonAsync<List<Category>>("/api/Category/GetAll");
            return View(categories);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var category = _http.GetFromJsonAsync<Category>($"/api/Category/GetByID/{id}");
            if (category == null)
                return NotFound();

            await _http.GetFromJsonAsync<Address>($"/api/Address/Delete/{category.Id}");
            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction("Categories");
        }

        [Authorize]
        public async Task<IActionResult> ExportUsersPDFAsync()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var users = await _http.GetFromJsonAsync<List<User>>("/api/User/GetAll");

            if (!users.Any())
            {
                TempData["Error"] = "No users available to export.";
                return RedirectToAction("Users");
            }

            string[] headers = {
                "الرقم القومي", "اسم المستخدم", "الاسم كامل", "النوع", "تاريخ الميلاد",
                "البريد الالكتروني", "الهاتف", "الوظيفة", "المدينة", "الشارع"
            };

            var data = users.Select(u => new
            {
                u.NationalId,
                u.UserName,
                Name = u.FirstName + " " + u.LastName,
                Gender = u.Gender == 'M' ? "Male" : "Female",
                BirthDate = u.BirthDate.Date.ToString("dd/MM/yyyy"),
                u.Email,
                u.PhoneNumber,
                Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault() ?? "N/A",
                City = _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{u.AddressId}").Result?.City ?? "N/A",
                Street = _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{u.AddressId}").Result?.Street ?? "N/A"
            }).Where(u => u.Role != "Admin").ToList();

            var exporter = new TableExporter();
            return exporter.ExportArabicToPDF(
                data,
                "Sany3y Users",
                headers,
                item => new object[] {
                    item.NationalId, item.UserName, item.Name, item.Gender, item.BirthDate,
                    item.Email, item.PhoneNumber, item.Role, item.City, item.Street
            });
        }

        [Authorize]
        public async Task<IActionResult> ExportUsersCSVAsync()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var users = await _http.GetFromJsonAsync<List<User>>("/api/User/GetAll");

            if (!users.Any())
            {
                TempData["Error"] = "No users available to export.";
                return RedirectToAction("Users");
            }

            string[] headers = {
                "National ID", "Username", "Full Name", "Gender", "Birth Date",
                "Email", "Phone", "Role", "City", "Street"
            };

            var data = users.Select(u => new
            {
                u.NationalId,
                u.UserName,
                Name = u.FirstName + " " + u.LastName,
                Gender = u.Gender == 'M' ? "Male" : "Female",
                BirthDate = u.BirthDate.Date.ToString("dd/MM/yyyy"),
                u.Email,
                u.PhoneNumber,
                Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault() ?? "N/A",
                City = _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{u.AddressId}").Result?.City ?? "N/A",
                Street = _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{u.AddressId}").Result?.Street ?? "N/A"
            }).Where(u => u.Role != "Admin").ToList();

            var exporter = new TableExporter();
            return exporter.ExportToCSV(
                data,
                "Sany3y Users",
                headers,
                item => new object[] {
                    item.NationalId, item.UserName, item.Name, item.Gender, item.BirthDate,
                    item.Email, item.PhoneNumber, item.Role, item.City, item.Street
                });
        }
        
        [Authorize]
        public async Task<IActionResult> ExportCategoriesPDF()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var categories = await _http.GetFromJsonAsync<List<Category>>("/api/Category/GetAll");

            if (!categories.Any())
            {
                TempData["Error"] = "No categories available to export.";
                return RedirectToAction("Categories");
            }

            string[] headers = { "ID", "الاسم", "الوصف" };

            var data = categories.Select(c => new
            {
                c.Id, c.Name, c.Description
            }).ToList();

            var exporter = new TableExporter();
            return exporter.ExportArabicToPDF(data, "Sany3y Categories", headers,
                item => new object[] { item.Id, item.Name, item.Description });
        }

        [Authorize]
        public async Task<IActionResult> ExportCategoriesCSVAsync()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var categories = await _http.GetFromJsonAsync<List<Category>>("/api/Category/GetAll");

            if (!categories.Any())
            {
                TempData["Error"] = "No categories available to export.";
                return RedirectToAction("Categories");
            }

            string[] headers = { "ID", "Name", "Description" };

            var data = categories.Select(c => new
            {
                c.Id, c.Name, c.Description
            }).ToList();

            var exporter = new TableExporter();
            return exporter.ExportToCSV(data, "Sany3y Categories", headers,
                item => new object[] { item.Id, item.Name, item.Description });
        }
    }
}
