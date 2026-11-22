using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sany3y.API.Services.CountryServices;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using Sany3y.Infrastructure.Services;
using Sany3y.Infrastructure.ViewModels;
using System.Security.Claims;

namespace Sany3y.API.Controllers
{
    [Route("api/Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<ProfilePicture> _pictureRepository;
        private readonly GovernorateServices _governorateServices;
        private readonly CityServices _cityServices;

        public AdminApiController(
            AppDbContext context,
            UserManager<User> userManager,
            IRepository<Address> addressRepository,
            IRepository<ProfilePicture> pictureRepository,
            GovernorateServices governorateServices,
            CityServices cityServices)
        {
            _context = context;
            _userManager = userManager;
            _addressRepository = addressRepository;
            _pictureRepository = pictureRepository;
            _governorateServices = governorateServices;
            _cityServices = cityServices;
        }

        private async Task<string> ResolveGovernorateNameAsync(string govInput)
        {
            if (int.TryParse(govInput, out int govId))
            {
                var gov = await _governorateServices.GetByID(govId);
                return gov?.ArabicName ?? govInput;
            }
            return govInput ?? "";
        }

        private async Task<string> ResolveCityNameAsync(string cityInput)
        {
            if (int.TryParse(cityInput, out int cityId))
            {
                var city = await _cityServices.GetByID(cityId);
                return city?.ArabicName ?? cityInput;
            }
            return cityInput ?? "";
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var allUsers = await _context.Users.ToListAsync();
            int totalClients = 0;
            int totalTaskers = 0;
            int totalStores = 0;

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                    continue;

                if (roles.Contains("Client") || roles.Contains("User"))
                    totalClients++;
                else if (user.IsShop == true)
                    totalStores++;
                else
                    totalTaskers++;
            }

            var totalCategories = await _context.Categories.CountAsync();

            var topCategories = await _context.Categories
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    count = _context.Tasks.Count(t => t.CategoryId == c.Id)
                })
                .OrderByDescending(c => c.count)
                .Take(6)
                .ToListAsync();

            return Ok(new
            {
                totalUsers = totalClients,
                totalTaskers,
                totalStores,
                totalCategories,
                topCategories
            });
        }

        [HttpGet("charts/monthly-users")]
        public async Task<IActionResult> GetMonthlyUserCounts()
        {
            var allUsers = await _context.Users.ToListAsync();
            var nonAdminUsers = new List<User>();

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (!roles.Contains("Admin"))
                    nonAdminUsers.Add(u);
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
                monthlyUserCounts.Add(new
                {
                    month = months[month - 1],
                    userCount = count
                });
            }

            return Ok(monthlyUserCounts);
        }

        [HttpGet("charts/gender-distribution")]
        public async Task<IActionResult> GetGenderDistribution()
        {
            var allUsers = await _context.Users.ToListAsync();
            var nonAdminUsers = new List<User>();

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (!roles.Contains("Admin"))
                    nonAdminUsers.Add(u);
            }

            var maleCount = nonAdminUsers.Count(u => u.Gender == 'M');
            var femaleCount = nonAdminUsers.Count(u => u.Gender == 'F');

            return Ok(new List<object>
            {
                new { gender = "ذكر", count = maleCount },
                new { gender = "أنثي", count = femaleCount }
            });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] bool? onlyShops)
        {
            var currentUserName = User.Identity?.Name;
            var query = _context.Users
                .Include(u => u.Address)
                .Include(u => u.ProfilePicture)
                .Include(u => u.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(currentUserName))
            {
                query = query.Where(u => u.UserName != currentUserName);
            }

            if (onlyShops == true)
            {
                query = query.Where(u => u.IsShop == true);
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            var result = new List<object>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new
                {
                    id = u.Id,
                    nationalId = u.NationalId,
                    userName = u.UserName,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    email = u.Email,
                    phoneNumber = u.PhoneNumber,
                    gender = u.Gender,
                    birthDate = u.BirthDate,
                    createdAt = u.CreatedAt,
                    isOnline = u.IsOnline,
                    isShop = u.IsShop == true,
                    shopName = u.ShopName,
                    categoryId = u.CategoryID,
                    categoryName = u.Category?.Name,
                    experienceYears = u.ExperienceYears,
                    price = u.Price,
                    bio = u.Bio,
                    roles = roles,
                    role = roles.FirstOrDefault() ?? "Client",
                    profilePictureUrl = u.ProfilePicture?.Path ?? "https://placehold.co/100x100?text=Profile",
                    address = u.Address != null ? new
                    {
                        id = u.Address.Id,
                        governorate = u.Address.Governorate,
                        city = u.Address.City,
                        street = u.Address.Street
                    } : null
                });
            }

            return Ok(result);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(long id)
        {
            var u = await _context.Users
                .Include(x => x.Address)
                .Include(x => x.ProfilePicture)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (u == null)
                return NotFound(new { message = "المستخدم غير موجود." });

            var roles = await _userManager.GetRolesAsync(u);
            return Ok(new
            {
                id = u.Id,
                nationalId = u.NationalId,
                userName = u.UserName,
                firstName = u.FirstName,
                lastName = u.LastName,
                email = u.Email,
                phoneNumber = u.PhoneNumber,
                gender = u.Gender,
                birthDate = u.BirthDate,
                createdAt = u.CreatedAt,
                isOnline = u.IsOnline,
                isShop = u.IsShop == true,
                shopName = u.ShopName,
                categoryId = u.CategoryID,
                categoryName = u.Category?.Name,
                experienceYears = u.ExperienceYears,
                price = u.Price,
                bio = u.Bio,
                roles = roles,
                role = roles.FirstOrDefault() ?? "Client",
                profilePictureUrl = u.ProfilePicture?.Path ?? "https://placehold.co/100x100?text=Profile",
                address = u.Address != null ? new
                {
                    id = u.Address.Id,
                    governorate = u.Address.Governorate,
                    city = u.Address.City,
                    street = u.Address.Street
                } : null
            });
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromForm] RegisterUserViewModel model, [FromForm] string role = "Client")
        {
            var govName = await ResolveGovernorateNameAsync(model.Governorate);
            var cityName = await ResolveCityNameAsync(model.City);

            var newAddress = new Address
            {
                Governorate = govName,
                City = cityName,
                Street = model.Street
            };
            await _addressRepository.Add(newAddress);

            var user = new User
            {
                NationalId = model.NationalId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true,
                Gender = model.IsMale ? 'M' : 'F',
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate,
                AddressId = newAddress.Id,
                CategoryID = role == "Technician" ? model.CategoryId : null,
                ExperienceYears = (role == "Technician" && model.IsShop != true) ? model.ExperienceYears : null,
                Price = (role == "Technician" && model.IsShop != true) ? model.Price : null,
                IsShop = model.IsShop,
                ShopName = model.IsShop == true ? model.ShopName : null
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                var errs = createResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { errors = errs, message = errs.FirstOrDefault() ?? "فشل إنشاء المستخدم." });
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return Ok(new { success = true, message = "تم إنشاء المستخدم بنجاح.", userId = user.Id });
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] AdminUpdateUserDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود." });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.BirthDate = dto.BirthDate;
            user.Bio = dto.Bio;
            user.CategoryID = dto.CategoryId;
            user.ExperienceYears = dto.ExperienceYears;
            user.Price = dto.Price;
            if (!string.IsNullOrWhiteSpace(dto.ShopName))
                user.ShopName = dto.ShopName;

            var govName = await ResolveGovernorateNameAsync(dto.Governorate);
            var cityName = await ResolveCityNameAsync(dto.City);

            if (user.Address != null)
            {
                if (!string.IsNullOrWhiteSpace(govName)) user.Address.Governorate = govName;
                if (!string.IsNullOrWhiteSpace(cityName)) user.Address.City = cityName;
                if (!string.IsNullOrWhiteSpace(dto.Street)) user.Address.Street = dto.Street;
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(dto.Role))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, dto.Role);
                }
            }

            return Ok(new { success = true, message = "تم تحديث بيانات المستخدم بنجاح." });
        }

        [HttpGet("export/users/{format}")]
        public async Task<IActionResult> ExportUsers(string format)
        {
            var users = await _context.Users.Include(u => u.Address).ToListAsync();
            var rows = new List<UserExportRow>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var role = roles.FirstOrDefault() ?? "N/A";
                if (role == "Admin") continue;

                rows.Add(new UserExportRow
                {
                    NationalId = u.NationalId,
                    UserName = u.UserName ?? "",
                    Name = $"{u.FirstName} {u.LastName}",
                    Gender = u.Gender == 'M' ? "Male" : "Female",
                    BirthDate = u.BirthDate.Date.ToString("dd/MM/yyyy"),
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber ?? "",
                    Role = role,
                    Governorate = u.Address?.Governorate ?? "N/A",
                    City = u.Address?.City ?? "N/A",
                    Street = u.Address?.Street ?? "N/A"
                });
            }

            var exporter = new TableExporter();
            if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
            {
                string[] headers = {
                    "الرقم القومي", "اسم المستخدم", "الاسم كامل", "النوع", "تاريخ الميلاد",
                    "البريد الالكتروني", "الهاتف", "الوظيفة", "المحافظة", "المدينة", "الشارع"
                };
                return exporter.ExportArabicToPDF(rows, "Sany3y Users", headers, item => new object[]
                {
                    item.NationalId, item.UserName, item.Name, item.Gender, item.BirthDate,
                    item.Email, item.PhoneNumber, item.Role, item.Governorate, item.City, item.Street
                });
            }
            else
            {
                string[] headers = {
                    "National ID", "Username", "Full Name", "Gender", "Birth Date",
                    "Email", "Phone", "Role", "Governorate", "City", "Street"
                };
                return exporter.ExportToCSV(rows, "Sany3y Users", headers, item => new object[]
                {
                    item.NationalId, item.UserName, item.Name, item.Gender, item.BirthDate,
                    item.Email, item.PhoneNumber, item.Role, item.Governorate, item.City, item.Street
                });
            }
        }

        [HttpGet("export/stores/{format}")]
        public async Task<IActionResult> ExportStores(string format)
        {
            var users = await _context.Users
                .Include(u => u.Address)
                .Include(u => u.Category)
                .Where(u => u.IsShop == true)
                .ToListAsync();

            var rows = users.Select(u => new
            {
                ShopName = u.ShopName ?? "",
                Category = u.Category?.Name ?? "N/A",
                OwnerName = $"{u.FirstName} {u.LastName}",
                UserName = u.UserName ?? "",
                CreationDate = u.BirthDate.Date.ToString("dd/MM/yyyy"),
                Email = u.Email ?? "",
                PhoneNumber = u.PhoneNumber ?? "",
                Governorate = u.Address?.Governorate ?? "N/A",
                City = u.Address?.City ?? "N/A",
                Street = u.Address?.Street ?? "N/A"
            }).ToList();

            var exporter = new TableExporter();
            if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
            {
                string[] headers = {
                    "اسم المتجر", "نوع النشاط", "اسم المالك", "اسم المستخدم", "تاريخ الإنشاء",
                    "البريد الالكتروني", "الهاتف", "المحافظة", "المدينة", "الشارع"
                };
                return exporter.ExportArabicToPDF(rows, "Sany3y Stores", headers, item => new object[]
                {
                    item.ShopName, item.Category, item.OwnerName, item.UserName, item.CreationDate,
                    item.Email, item.PhoneNumber, item.Governorate, item.City, item.Street
                });
            }
            else
            {
                string[] headers = {
                    "Shop Name", "Category", "Owner Name", "Username", "Creation Date",
                    "Email", "Phone", "Governorate", "City", "Street"
                };
                return exporter.ExportToCSV(rows, "Sany3y Stores", headers, item => new object[]
                {
                    item.ShopName, item.Category, item.OwnerName, item.UserName, item.CreationDate,
                    item.Email, item.PhoneNumber, item.Governorate, item.City, item.Street
                });
            }
        }

        [HttpGet("export/categories/{format}")]
        public async Task<IActionResult> ExportCategories(string format)
        {
            var categories = await _context.Categories.ToListAsync();
            var exporter = new TableExporter();

            if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
            {
                string[] headers = { "ID", "الاسم", "الوصف" };
                return exporter.ExportArabicToPDF(categories, "Sany3y Categories", headers,
                    item => new object[] { item.Id, item.Name, item.Description ?? "" });
            }
            else
            {
                string[] headers = { "ID", "Name", "Description" };
                return exporter.ExportToCSV(categories, "Sany3y Categories", headers,
                    item => new object[] { item.Id, item.Name, item.Description ?? "" });
            }
        }

        private class UserExportRow
        {
            public long NationalId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Gender { get; set; } = string.Empty;
            public string BirthDate { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string Governorate { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Street { get; set; } = string.Empty;
        }
    }

    public class AdminUpdateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? Bio { get; set; }
        public int? CategoryId { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? Price { get; set; }
        public string? ShopName { get; set; }
        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Role { get; set; } = "Client";
    }
}
