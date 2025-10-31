using System.Security.Cryptography;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly UserRepository _userRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<ProfilePicture> _profilePictureRepository;
        private readonly IRepository<Category> _categoryRepository;

        public AdminController(
            UserManager<User> userManager,
            UserRepository userRepository,
            SignInManager<User> signInManager,
            IRepository<Address> addressRepository,
            IRepository<Category> categoryRepository,
            IRepository<ProfilePicture> profilePictureRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _signInManager = signInManager;
            _addressRepository = addressRepository;
            _categoryRepository = categoryRepository;
            _profilePictureRepository = profilePictureRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var totalUsers = await _userRepository.GetAll();
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

            var totalCategories = await _categoryRepository.GetAll();
            ViewBag.TotalCategories = totalCategories.Count;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> GetMonthlyUserCounts()
        {
            var allUsers = await _userRepository.GetAll();
            var nonAdminUsers = new List<User>();

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (!roles.Contains("Admin")) nonAdminUsers.Add(u);
            }

            var currentYear = DateTime.Now.Year;
            var months = new List<string>
            {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            };

            var monthlyUserCounts = new List<object>();
            for (int month = 1; month <= 12; month++)
            {
                var count = nonAdminUsers.Count(u => u.CreatedAt.Year == currentYear && u.CreatedAt.Month == month);
                monthlyUserCounts.Add(new Dictionary<string, object>
                {
                    { "Month", months[month - 1] },
                    { "UserCount", count }
                });
            }

            return Json(monthlyUserCounts);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Users()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var currentUser = await _userManager.GetUserAsync(User);
            var users = (await _userRepository.GetAll()).Where(u => u.UserName != currentUser?.UserName);
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

            var user = await _userRepository.GetById(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Address = await _addressRepository.GetById(user.AddressId);
            var viewModel = (User: user, Roles: roles);
            return PartialView("_ViewUserModal", viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var user = await _userRepository.GetById(id);
            if (user == null)
                return NotFound();

            await _userRepository.Delete(user);
            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction("Users");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Categories()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var categories = _categoryRepository.GetAll().Result;
            return View(categories);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var category = await _categoryRepository.GetById(id);
            if (category == null)
                return NotFound();

            await _categoryRepository.Delete(category);
            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction("Categories");
        }

        [Authorize]
        public IActionResult ExportUsersPDF()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var users = _userRepository.GetAll().Result;

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
                City = _addressRepository.GetById(u.AddressId).Result?.City ?? "N/A",
                Street = _addressRepository.GetById(u.AddressId).Result?.Street ?? "N/A"
            }).Where(u => u.Role != "Admin").ToList();

            var exporter = new TableExporter();
            return exporter.ExportToPDF(
                data,
                "Sany3y Users",
                headers,
                item => new object[] {
                    item.NationalId, item.UserName, item.Name, item.Gender, item.BirthDate,
                    item.Email, item.PhoneNumber, item.Role, item.City, item.Street
                });
        }

        [Authorize]
        public IActionResult ExportUsersCSV()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var users = _userRepository.GetAll().Result;

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
                City = _addressRepository.GetById(u.AddressId).Result?.City ?? "N/A",
                Street = _addressRepository.GetById(u.AddressId).Result?.Street ?? "N/A"
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
        public IActionResult ExportCategoriesPDF()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var categories = _categoryRepository.GetAll().Result;

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
            return exporter.ExportToPDF(data, "Sany3y Categories", headers,
                item => new object[] { item.Id, item.Name, item.Description });
        }

        [Authorize]
        public IActionResult ExportCategoriesCSV()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var categories = _categoryRepository.GetAll().Result;

            if (!categories.Any())
            {
                TempData["Error"] = "No categories available to export.";
                return RedirectToAction("Categories");
            }

            string[] headers = { "ID", "Name", "Description" };

            var data = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description
            }).ToList();

            var exporter = new TableExporter();
            return exporter.ExportToCSV(data, "Sany3y Categories", headers,
                item => new object[] { item.Id, item.Name, item.Description });
        }
    }
}
