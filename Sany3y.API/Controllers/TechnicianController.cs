using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.ViewModels;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechnicianController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public TechnicianController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("GetAll")]
        public async Task<List<User>> GetAll()
        {
            var technicians = await _userManager.GetUsersInRoleAsync("Technician");
            var techIds = technicians.Select(t => t.Id).ToList();
            
            var usersWithDetails = await _context.Users
                .Include(u => u.Address)
                .Include(u => u.ProfilePicture)
                .Include(u => u.Category)
                .Where(u => techIds.Contains(u.Id))
                .ToListAsync();

            return usersWithDetails;
        }

        public static string NormalizeArabic(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string normalized = text
                .Replace("أ", "ا")
                .Replace("إ", "ا")
                .Replace("آ", "ا")
                .Replace("ة", "ه")
                .Replace("ى", "ي")
                .Replace("ؤ", "و")
                .Replace("ئ", "ي");

            string diacritics = @"[\u064B-\u0652]";
            return System.Text.RegularExpressions.Regex.Replace(normalized, diacritics, "");
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search(
            [FromQuery] bool? isShop,
            [FromQuery] int? categoryId,
            [FromQuery] string? governorate,
            [FromQuery] string? city,
            [FromQuery] string? name,
            [FromQuery] string? serviceType,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] double? rating,
            [FromQuery] string? sortBy)
        {
            var technicians = await _userManager.GetUsersInRoleAsync("Technician");
            var techIds = technicians.Select(t => t.Id).ToList();

            var users = await _context.Users
                .Include(u => u.Address)
                .Include(u => u.ProfilePicture)
                .Include(u => u.Category)
                .Where(u => techIds.Contains(u.Id))
                .ToListAsync();

            if (isShop.HasValue)
            {
                if (isShop.Value)
                    users = users.Where(u => u.IsShop == true).ToList();
                else
                    users = users.Where(u => u.IsShop == null || u.IsShop == false).ToList();
            }

            if (!string.IsNullOrWhiteSpace(serviceType) && categoryId == null)
            {
                var norm = NormalizeArabic(serviceType.Trim());
                categoryId = norm switch
                {
                    "بناء وتشييد" => 1,
                    "كهربا" or "كهرباء" => 2,
                    "سباكه" => 3,
                    "دهانات و تشطيبات" or "دهانات وتشطيبات" => 4,
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
                    "خدمات اخرى" or "خدمات اخري" => 16,
                    _ => null
                };
                if (categoryId == null)
                {
                    users = users.Where(u =>
                        (u.Category != null && NormalizeArabic(u.Category.Name).Contains(norm)) ||
                        NormalizeArabic($"{u.FirstName} {u.LastName}").Contains(norm) ||
                        (u.Bio != null && NormalizeArabic(u.Bio).Contains(norm))
                    ).ToList();
                }
            }

            if (categoryId.HasValue && categoryId.Value > 0)
                users = users.Where(u => u.CategoryID == categoryId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(governorate))
                users = users.Where(u => u.Address != null && u.Address.Governorate.Contains(governorate)).ToList();

            if (!string.IsNullOrWhiteSpace(city))
                users = users.Where(u => u.Address != null && u.Address.City.Contains(city)).ToList();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var normName = NormalizeArabic(name.Trim());
                users = users.Where(u =>
                    (u.ShopName != null && NormalizeArabic(u.ShopName).Contains(normName)) ||
                    NormalizeArabic($"{u.FirstName} {u.LastName}").Contains(normName)
                ).ToList();
            }

            if (minPrice.HasValue)
                users = users.Where(u => (u.Price ?? 0) >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                users = users.Where(u => (u.Price ?? 0) <= maxPrice.Value).ToList();

            var allRatings = await _context.Ratings
                .Where(r => techIds.Contains(r.TaskerId))
                .ToListAsync();

            var results = users.Select(u =>
            {
                var uRatings = allRatings.Where(r => r.TaskerId == u.Id).ToList();
                double avgRating = uRatings.Any() ? uRatings.Average(r => r.Score) : (u.Rating ?? 4.5);
                return new
                {
                    id = u.Id,
                    userName = u.UserName,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    email = u.Email,
                    phoneNumber = u.PhoneNumber,
                    bio = u.Bio,
                    isOnline = u.IsOnline,
                    isShop = u.IsShop == true,
                    shopName = u.ShopName,
                    categoryId = u.CategoryID,
                    categoryName = u.Category?.Name ?? "غير محدد",
                    experienceYears = u.ExperienceYears ?? 5,
                    price = u.Price ?? 50,
                    rating = Math.Round(avgRating, 1),
                    reviewCount = uRatings.Count,
                    profilePictureUrl = u.ProfilePicture?.Path,
                    governorate = u.Address?.Governorate ?? "القاهرة",
                    city = u.Address?.City ?? "غير محدد",
                    street = u.Address?.Street ?? "غير محدد",
                    address = u.Address
                };
            }).ToList();

            if (rating.HasValue)
                results = results.Where(r => r.rating >= rating.Value).ToList();

            if (sortBy == "rating")
                results = results.OrderByDescending(r => r.rating).ToList();
            else if (sortBy == "price_asc")
                results = results.OrderBy(r => r.price).ToList();

            return Ok(results);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> GetDetails(long id)
        {
            var user = await _context.Users
                .Include(u => u.Address)
                .Include(u => u.ProfilePicture)
                .Include(u => u.Category)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "الفني أو المحل غير موجود." });

            var ratings = await _context.Ratings
                .Include(r => r.Client)
                .Where(r => r.TaskerId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    id = r.Id,
                    score = r.Score,
                    comment = r.Comment,
                    createdAt = r.CreatedAt,
                    clientId = r.ClientId,
                    clientName = r.Client != null ? $"{r.Client.FirstName} {r.Client.LastName}" : "عميل مستخدم"
                })
                .ToListAsync();

            double averageRating = ratings.Any() ? ratings.Average(r => r.score) : (user.Rating ?? 0);

            return Ok(new
            {
                id = user.Id,
                userName = user.UserName,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                bio = user.Bio,
                createdAt = user.CreatedAt,
                isOnline = user.IsOnline,
                isShop = user.IsShop == true,
                shopName = user.ShopName,
                categoryId = user.CategoryID,
                categoryName = user.Category?.Name ?? (user.IsShop == true ? "محل عام" : "صنايعي عام"),
                experienceYears = user.ExperienceYears ?? 5,
                price = user.Price ?? 50,
                averageRating = Math.Round(averageRating, 1),
                reviewCount = ratings.Count,
                profilePictureUrl = user.ProfilePicture?.Path,
                address = user.Address != null ? new
                {
                    id = user.Address.Id,
                    governorate = user.Address.Governorate,
                    city = user.Address.City,
                    street = user.Address.Street
                } : null,
                ratings = ratings
            });
        }

        [HttpGet("GetByID/{id}")]
        public async Task<ActionResult<User>> GetByID(int id)
        {
            var allTechnician = await _userManager.GetUsersInRoleAsync("Technician");
            var techIds = allTechnician.Select(t => t.Id).ToList();

            var technician = await _context.Users
                .Include(u => u.Address)
                .Include(u => u.ProfilePicture)
                .Include(u => u.Category)
                .FirstOrDefaultAsync(t => t.Id == id && techIds.Contains(t.Id));

            if (technician == null)
                return NotFound();

            return technician;
        }

        [HttpGet("GetByCategory/{categoryId}")]
        public async Task<ActionResult<List<User>>> GetByCategory(int categoryId)
        {
            var allTechnicians = await _userManager.GetUsersInRoleAsync("Technician");
            var techIds = allTechnicians.Select(t => t.Id).ToList();

            var query = _context.Users
                .Include(u => u.Address)
                .Include(u => u.ProfilePicture)
                .Include(u => u.Category)
                .Where(u => techIds.Contains(u.Id));

            if (categoryId != 0)
            {
                query = query.Where(t => t.CategoryID == categoryId);
            }

            return await query.ToListAsync();
        }

        [HttpPost("Create")]
        public async Task<ActionResult<User>> Create(TechnicianViewModel technician)
        {
            var newAddress = new Address
            {
                Street = technician.Street,
                City = technician.City,
                Governorate = technician.Governorate
            };
            await _context.Addresses.AddAsync(newAddress);
            await _context.SaveChangesAsync();

            var newTechnician = new User
            {
                NationalId = technician.NationalId,
                FirstName = technician.FirstName,
                LastName = technician.LastName,
                UserName = technician.UserName,
                BirthDate = technician.BirthDate,
                Bio = technician.Bio,
                Gender = technician.IsMale ? 'M' : 'F',
                Email = technician.Email,
                PhoneNumber = technician.PhoneNumber,
                ExperienceYears = technician.ExperienceYears,
                Price = technician.Price,
                Rating = technician.Rating,
                AddressId = newAddress.Id,
                CategoryID = technician.CategoryID
            };

            await _userManager.CreateAsync(newTechnician, technician.Password);
            await _userManager.AddToRoleAsync(newTechnician, "Technician");
            return CreatedAtAction(nameof(GetByID), new { id = newTechnician.Id }, technician);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, TechnicianViewModel technician)
        {
            var allTechnician = await _userManager.GetUsersInRoleAsync("Technician");
            var existingTechnician = allTechnician.FirstOrDefault(t => t.Id == id);

            if (existingTechnician == null)
                return NotFound();

            if (existingTechnician.NationalId != technician.NationalId)
                return BadRequest();

            existingTechnician.FirstName = technician.FirstName;
            existingTechnician.LastName = technician.LastName;
            existingTechnician.BirthDate = technician.BirthDate;
            existingTechnician.Bio = technician.Bio;
            existingTechnician.Gender = technician.IsMale ? 'M' : 'F';
            existingTechnician.PhoneNumber = technician.PhoneNumber;
            existingTechnician.ExperienceYears = technician.ExperienceYears;
            existingTechnician.Price = technician.Price;

            _context.Entry(existingTechnician).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        [HttpPut]
        [Route("UpdateCategory/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] int categoryId)
        {
            var allTechnician = await _userManager.GetUsersInRoleAsync("Technician");
            var existingTechnician = allTechnician.FirstOrDefault(t => t.Id == id);
            if (existingTechnician == null)
                return NotFound();

            existingTechnician.CategoryID = categoryId;
            _context.Entry(existingTechnician).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var allTechnician = await _userManager.GetUsersInRoleAsync("Technician");
            var technician = allTechnician.FirstOrDefault(t => t.Id == id);

            if (technician == null)
                return NotFound();

            await _userManager.DeleteAsync(technician);
            return NoContent();
        }
    }
}