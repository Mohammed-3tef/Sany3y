using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sany3y.API.DTOs;
using Sany3y.Infrastructure.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("customer/{userId}")]
        public async Task<ActionResult<CustomerDashboardDto>> GetCustomerDashboard(long userId)
        {
            var user = await _context.Users
                .Include(u => u.Address)
                .Include(u => u.ProfilePicture)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var tasks = await _context.Tasks
                .Include(t => t.Tasker)
                .Include(t => t.Category)
                .Where(t => t.ClientId == userId)
                .ToListAsync();

            var pastBookings = tasks
                .Where(t => t.Status == "Completed" || t.Status == "Cancelled")
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    OtherPartyName = t.Tasker != null ? $"{t.Tasker.FirstName} {t.Tasker.LastName}" : "Unknown",
                    Price = t.Tasker?.Price ?? 0,
                    ServiceName = t.Category?.Name ?? "Service"
                }).ToList();

            var upcomingAppointments = tasks
                .Where(t => t.Status == "Pending" || t.Status == "Accepted" || t.Status == "In Progress" || t.Status == "Paid")
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    OtherPartyName = t.Tasker != null ? $"{t.Tasker.FirstName} {t.Tasker.LastName}" : "Unknown",
                    Price = (t.Tasker?.Price ?? 0) > 0 ? t.Tasker.Price.Value : 50, // Default to 50 if null OR zero
                    ServiceName = t.Category?.Name ?? "Service",
                    PaymentMethodId = t.PaymentMethodId
                }).ToList();

            var dashboard = new CustomerDashboardDto
            {
                Profile = new UserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Bio = user.Bio,
                    Location = user.Address != null ? $"{user.Address.City}, {user.Address.Street}" : "No Address",
                    ProfilePictureUrl = user.ProfilePicture != null ? user.ProfilePicture.Path : null
                },
                PastBookings = pastBookings,
                UpcomingAppointments = upcomingAppointments
            };

            return Ok(dashboard);
        }

        [HttpGet("worker/{userId}")]
        public async Task<ActionResult<WorkerDashboardDto>> GetWorkerDashboard(long userId)
        {
            var user = await _context.Users
                .Include(u => u.Address)
                .Include(u => u.ProfilePicture)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var tasks = await _context.Tasks
                .Include(t => t.Client)
                .Where(t => t.TaskerId == userId)
                .ToListAsync();

            var receivedRequests = tasks
                .Where(t => t.Status == "Pending")
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    OtherPartyName = t.Client != null ? $"{t.Client.FirstName} {t.Client.LastName}" : "Unknown"
                }).ToList();

            var acceptedJobs = tasks
                .Where(t => t.Status == "Accepted" || t.Status == "In Progress")
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    OtherPartyName = t.Client != null ? $"{t.Client.FirstName} {t.Client.LastName}" : "Unknown"
                }).ToList();

            // Calculate earnings from Payments table
            var totalEarnings = await _context.Payments
                .Include(p => p.Task)
                .Where(p => p.Task.TaskerId == userId && p.PaymentStatus == "Completed")
                .SumAsync(p => p.AmountPaid);

            var dashboard = new WorkerDashboardDto
            {
                Profile = new UserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Bio = user.Bio,
                    Location = user.Address != null ? $"{user.Address.City}, {user.Address.Street}" : "No Address",
                    ProfilePictureUrl = user.ProfilePicture != null ? user.ProfilePicture.Path : null,
                    ShopName = user.ShopName,
                    IsShop = user.IsShop
                },
                ReceivedRequests = receivedRequests,
                AcceptedJobs = acceptedJobs,
                TotalEarnings = totalEarnings
            };

            return Ok(dashboard);
        }

        [HttpPut("profile/{userId}")]
        public async Task<IActionResult> UpdateProfile(long userId, [FromBody] UpdateProfileDto updateDto)
        {
            var user = await _context.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.PhoneNumber = updateDto.Phone;
            user.Bio = updateDto.Bio;

            // Handle Address update simply for now
            if (user.Address == null)
            {
                user.Address = new Address(); // Assuming Address has a parameterless constructor or we need to init it properly
                // Ideally we'd parse the location string or have separate fields in DTO
                user.Address.City = updateDto.Location; // Just dumping it here for simplicity as per requirements
                user.Address.Street = "";
            }
            else
            {
                user.Address.City = updateDto.Location;
            }

            // Service Details might be bio or category, updating Bio for now as per DTO
            // If ServiceDetails maps to something else, we'd update it here.

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
