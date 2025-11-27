using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sany3y.Infrastructure.Models;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechnicianScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TechnicianScheduleController(AppDbContext context)
        {
            _context = context;
        }

        // ----------------------------------
        // ✅ Get schedule for specific technician
        // ----------------------------------
        [HttpGet("GetSchedule/{technicianId}")]
        public async Task<IActionResult> GetSchedule(long technicianId)
        {
            var schedule = await _context.TechnicianSchedules
                .Where(s => s.TechnicianId == technicianId && !s.IsBooked)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return Ok(schedule);
        }

        // ----------------------------------
        // ✅ Book a slot
        // ----------------------------------
        [HttpPost("BookSlot")]
        public async Task<IActionResult> BookSlot([FromBody] BookingRequest req)
        {
            var slot = await _context.TechnicianSchedules
                .FirstOrDefaultAsync(s => s.Id == req.ScheduleId);

            if (slot == null)
                return NotFound("Slot not found");

            if (slot.IsBooked)
                return BadRequest("Slot already booked");

            slot.IsBooked = true;
            await _context.SaveChangesAsync();

            return Ok("Booking confirmed");
        }
    }

    // Request model
    public class BookingRequest
    {
        public int ScheduleId { get; set; }
        public long CustomerId { get; set; }
    }
}
