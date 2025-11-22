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
        public async Task<List<User>?> GetAll()
        {
            return await _userManager.GetUsersInRoleAsync("Technician") as List<User>;
        }

        [HttpGet("GetByID/{id}")]
        public async Task<ActionResult<User>> GetByID(int id)
        {
            var allTechnician = await _userManager.GetUsersInRoleAsync("Technician");
            var technician = allTechnician.FirstOrDefault(t => t.Id == id);

            if (technician == null)
                return NotFound();

            return technician;
        }

        [HttpPost("Create")]
        public async Task<ActionResult<User>> Create(TechnicianViewModel technician)
        {
            var newAddress = new Address
            {
                Street = technician.Street,
                City = technician.City
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