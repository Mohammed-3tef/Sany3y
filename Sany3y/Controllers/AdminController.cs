using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.DTOs;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Services;
using Sany3y.Infrastructure.ViewModels;
using Sany3y.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Sany3y.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _http;
        private readonly UserManager<User> _userManager;

        #region Helpers

        private async System.Threading.Tasks.Task PopulateGovernoratesAsync()
        {
            var governorates = await _http.GetFromJsonAsync<List<Governorate>>("/api/CountryServices/GetAllGovernorates");
            ViewBag.AllGovernorates = governorates?.OrderBy(g => g.ArabicName).ToList();
        }

        private async System.Threading.Tasks.Task GetAllCategories()
        {
            ViewBag.AllCategories = await _http.GetFromJsonAsync<List<Category>>("/api/Category/GetAll");
        }

        #endregion

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var totalUsers = await _http.GetFromJsonAsync<List<User>>("/api/User/GetAll");
            var totalUserCount = 0;
            var totalTaskerCount = 0;

            foreach (var user in totalUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User"))
                    totalUserCount++;
                if (roles.Contains("Technician"))
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var currentUser = await _http.GetFromJsonAsync<User>($"/api/User/GetByUserName/{User?.Identity?.Name}");
            var users = (await _http.GetFromJsonAsync<List<User>>($"/api/User/GetAll"))?.Where(u => u.UserName != currentUser?.UserName);
            var userRoles = new List<(User User, IList<string> Roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add((user, roles));
            }

            ViewBag.JwtToken = HttpContext.Session.GetString("JwtToken") ?? "";
            return View(userRoles);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ViewUser(int id)
        {
            var user = await _http.GetFromJsonAsync<User>($"/api/User/GetByID/{id}");
            if (user == null)
                return NotFound();

            ViewBag.Address = await _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{user.AddressId}");
            var userCategory = await _http.GetFromJsonAsync<Category>($"/api/Category/GetByID/{user.CategoryID}");
            ViewBag.UserCategory = userCategory?.Name;

            var response = await _http.GetAsync($"/api/ProfilePicture/GetByID/{user.ProfilePictureId}");
            if (response.IsSuccessStatusCode)
            {
                var profilePicture = await response.Content.ReadFromJsonAsync<ProfilePicture>();
                ViewBag.UserProfilePictureUrl = profilePicture?.Path ?? "https://placehold.co/100x100?text=Profile";
            }
            else
            {
                ViewBag.UserProfilePictureUrl = "https://placehold.co/100x100?text=Profile";
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = (User: user, Roles: roles);
            return PartialView("_ViewUserModal", viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUser()
        {
            var roles = await _http.GetFromJsonAsync<List<Role>>($"/api/Role/GetAll");
            ViewBag.Roles = roles?.Select(r => r.Name).ToList();
            ViewBag.JwtToken = HttpContext.Session.GetString("JwtToken") ?? "";
            await PopulateGovernoratesAsync();
            await GetAllCategories();
            return PartialView("_AddUserModal");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUser([FromForm] RegisterUserViewModel user, [FromForm] string role)
        {
            if (!ModelState.IsValid)
                return View("Users", user);

            using var form = new MultipartFormDataContent();

            // نصوص وأرقام
            form.Add(new StringContent(user.NationalId.ToString()), "NationalId");
            form.Add(new StringContent(user.FirstName), "FirstName");
            form.Add(new StringContent(user.LastName), "LastName");
            form.Add(new StringContent(user.UserName), "UserName");

            // تاريخ بصيغة متوافقة
            form.Add(new StringContent(user.BirthDate.ToString("yyyy-MM-ddTHH:mm:ss")), "BirthDate");

            // Boolean
            form.Add(new StringContent(user.IsMale.ToString()), "IsMale");
            form.Add(new StringContent((role == "Client").ToString()), "IsClient");
            if (!string.IsNullOrEmpty(user.CategoryId.ToString()))
                form.Add(new StringContent(user.CategoryId.ToString()), "CategoryId");
            if (!string.IsNullOrEmpty(user.ExperienceYears.ToString()))
                form.Add(new StringContent(user.ExperienceYears.ToString()), "ExperienceYears");
            if (!string.IsNullOrEmpty(user.Price.ToString()))
                form.Add(new StringContent(user.Price.ToString()), "Price");

            // بقية البيانات
            form.Add(new StringContent(user.Email), "Email");
            form.Add(new StringContent(user.PhoneNumber), "PhoneNumber");
            form.Add(new StringContent(user.Password), "Password");
            form.Add(new StringContent(user.ConfirmPassword), "ConfirmPassword");
            form.Add(new StringContent(user.Governorate), "Governorate");
            form.Add(new StringContent(user.City), "City");
            form.Add(new StringContent(user.Street), "Street");

            // Picture لو موجود
            if (!string.IsNullOrEmpty(user.Picture))
                form.Add(new StringContent(user.Picture), "Picture");

            // الملف
            if (user.NationalIdImage != null)
            {
                var fileContent = new StreamContent(user.NationalIdImage.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(user.NationalIdImage.ContentType);
                form.Add(fileContent, "NationalIdImage", user.NationalIdImage.FileName);
            }

            // إرسال البيانات للـ API
            var response = await _http.PostAsync("/api/User/Create", form);
            if (!response.IsSuccessStatusCode)
                return BadRequest(new { error = "Failed to create user." });

            var createdUser = await response.Content.ReadFromJsonAsync<User>();

            // إضافة الدور
            if (createdUser != null)
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JwtToken"));

                var roleResponse = await _http.PutAsJsonAsync($"/api/User/UpdateRole/{createdUser.Id}", role);
                if (!roleResponse.IsSuccessStatusCode)
                    return BadRequest(new { error = "Failed to assign user role." });
            }

            return Ok(new { success = true, message = "User created successfully." });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditUser(int id)
        {
            var user = await _http.GetFromJsonAsync<User>($"/api/User/GetByID/{id}");
            if (user == null)
                return NotFound();

            ViewBag.Address = await _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{user.AddressId}");

            var roles = await _http.GetFromJsonAsync<List<Role>>($"/api/Role/GetAll");
            ViewBag.Roles = roles?.Select(r => r.Name).ToList();
            var userRole = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = userRole.FirstOrDefault()?.ToString();

            var response = await _http.GetAsync($"/api/ProfilePicture/GetByID/{user.ProfilePictureId}");
            if (response.IsSuccessStatusCode)
            {
                var profilePicture = await response.Content.ReadFromJsonAsync<ProfilePicture>();
                ViewBag.UserProfilePictureUrl = profilePicture?.Path ?? "https://placehold.co/100x100?text=Profile";
            }
            else
            {
                ViewBag.UserProfilePictureUrl = "https://placehold.co/100x100?text=Profile";
            }

            await PopulateGovernoratesAsync();
            await GetAllCategories();
            ViewBag.JwtToken = HttpContext.Session.GetString("JwtToken") ?? "";
            return PartialView("_EditUserModal", user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser([FromForm] string user, [FromForm] string address, [FromForm] IFormFile profilePicture, [FromForm] string role)
        {
            var userObj = JsonSerializer.Deserialize<UserUpdateDTO>(user);
            var addressObj = JsonSerializer.Deserialize<Address>(address);

            if (userObj == null)
                return BadRequest("User is null");

            // ----- التعامل مع العنوان -----
            var existingAddress = await _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{addressObj?.Id}");
            var addressCreateResponse = await _http.PutAsJsonAsync($"/api/Address/Update/{existingAddress?.Id}", addressObj);
            if (!addressCreateResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "فشل في إنشاء/تحديث العنوان.";
                return RedirectToAction("Users");
            }

            // ----- تحديث بيانات المستخدم -----
            var response = await _http.PutAsJsonAsync($"/api/User/Update/{userObj.Id}", userObj);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "فشل في تحديث بيانات المستخدم.";
                return RedirectToAction("Users");
            }

            // ----- تحديث الدور -----
            var updatedUser = await response.Content.ReadFromJsonAsync<User>();
            if (updatedUser != null)
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JwtToken"));
                var roleResponse = await _http.PutAsJsonAsync($"/api/User/UpdateRole/{updatedUser.Id}", role);
                if (!roleResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "فشل في تحديث دور المستخدم.";
                    return RedirectToAction("Users");
                }
            }

            return RedirectToAction("Users");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Categories()
        {
            var categories = await _http.GetFromJsonAsync<List<Category>>("/api/Category/GetAll");
            ViewBag.JwtToken = HttpContext.Session.GetString("JwtToken") ?? "";
            return View(categories);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportUsersPDFAsync()
        {
            var users = await _http.GetFromJsonAsync<List<User>>("/api/User/GetAll");

            if (!users.Any())
            {
                TempData["Error"] = "لا يوجد مستخدمون متاحون للتصدير.";
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
                Street = _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{u.AddressId}").Result?.Street ?? "N/A",
                Governorate = _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{u.AddressId}").Result?.Governorate ?? "N/A"
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportUsersCSVAsync()
        {
            var users = await _http.GetFromJsonAsync<List<User>>("/api/User/GetAll");

            if (!users.Any())
            {
                TempData["Error"] = "لا يوجد مستخدمون متاحون للتصدير.";
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCategoriesPDF()
        {
            var categories = await _http.GetFromJsonAsync<List<Category>>("/api/Category/GetAll");

            if (!categories.Any())
            {
                TempData["Error"] = "لا توجد تصنيفات متاحة للتصدير.";
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCategoriesCSVAsync()
        {
            var categories = await _http.GetFromJsonAsync<List<Category>>("/api/Category/GetAll");

            if (!categories.Any())
            {
                TempData["Error"] = "لا توجد تصنيفات متاحة للتصدير.";
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
