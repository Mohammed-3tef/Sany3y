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
        // ----------------------------------
        // ✅ Book a slot
        // ----------------------------------
        [HttpPost("BookSlot")]
        public async Task<IActionResult> BookSlot([FromBody] BookingRequest req)
        {
            var slot = await _context.TechnicianSchedules
                .Include(s => s.Technician)
                .FirstOrDefaultAsync(s => s.Id == req.ScheduleId);

            if (slot == null)
                return NotFound("Slot not found");

            if (slot.IsBooked)
                return BadRequest("Slot already booked");

            // Prevent Self-Booking
            if (slot.TechnicianId == req.CustomerId)
                return BadRequest("You cannot book your own slot.");

            // 1. Mark Slot as Booked (Reserved pending approval)
            slot.IsBooked = true;

            // 2. Create a Task (Job)
            var newTask = new Sany3y.Infrastructure.Models.Task
            {
                Title = $"Booking on {slot.Date:yyyy-MM-dd}",
                Description = $"Scheduled appointment from {slot.StartTime} to {slot.EndTime}",
                Status = "Pending", // Changed to Pending for approval workflow
                CategoryId = slot.Technician.CategoryID ?? 1,
                ClientId = req.CustomerId,
                TaskerId = slot.TechnicianId
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            return Ok("Booking request sent. Waiting for technician approval.");
        }

        // ----------------------------------
        // ✅ Accept Request
        // ----------------------------------
        [HttpPost("AcceptRequest/{taskId}")]
        public async Task<IActionResult> AcceptRequest(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Tasker)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return NotFound("Task not found");
            if (task.Status != "Pending") return BadRequest("Task is not pending");

            // 1. Update Status
            task.Status = "Accepted";

            // 2. Create Payment (Now we create it upon acceptance)
            var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync();
            var paymentMethodId = paymentMethod?.Id ?? 1;
            
            // Ensure PaymentMethod exists if not found (Safety check)
            if (paymentMethod == null)
            {
                 var defaultMethod = new PaymentMethod { Name = "Cash" };
                 _context.PaymentMethods.Add(defaultMethod);
                 await _context.SaveChangesAsync();
                 paymentMethodId = defaultMethod.Id;
            }

            var price = task.Tasker.Price ?? 0;

            var newPayment = new Payment
            {
                AmountAgreed = price,
                AmountPaid = price,
                PaymentDate = DateTime.Now,
                PaymentStatus = "Completed",
                PaymentMethodId = paymentMethodId,
                TaskId = task.Id,
                ClientId = task.ClientId
            };

            _context.Payments.Add(newPayment);
            await _context.SaveChangesAsync();

            return Ok("Request accepted");
        }

        // ----------------------------------
        // ✅ Reject Request
        // ----------------------------------
        [HttpPost("RejectRequest/{taskId}")]
        public async Task<IActionResult> RejectRequest(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return NotFound("Task not found");
            if (task.Status != "Pending") return BadRequest("Task is not pending");

            // 1. Update Status
            task.Status = "Rejected";

            // 2. Free up the slot
            // We need to find the slot. We can try to match by Date, StartTime, and TechnicianId
            // Parsing Title/Description is fragile but necessary without a direct link
            // Title format: "Booking on yyyy-MM-dd"
            // Description format: "Scheduled appointment from HH:mm:ss to HH:mm:ss"

            try 
            {
                var dateStr = task.Title.Replace("Booking on ", "");
                var date = DateTime.Parse(dateStr);

                // Extract StartTime from Description
                // "Scheduled appointment from 09:00:00 to 10:00:00"
                var parts = task.Description.Split(" from ");
                if (parts.Length > 1)
                {
                    var times = parts[1].Split(" to ");
                    var startTimeStr = times[0];
                    var startTime = TimeSpan.Parse(startTimeStr);

                    var slot = await _context.TechnicianSchedules
                        .FirstOrDefaultAsync(s => 
                            s.TechnicianId == task.TaskerId && 
                            s.Date.Date == date.Date && 
                            s.StartTime == startTime);

                    if (slot != null)
                    {
                        slot.IsBooked = false;
                    }
                }
            }
            catch
            {
                // If parsing fails, we just reject the task but might fail to free the slot automatically.
                // In a real app, we should have a ScheduleId in the Task table.
            }

            await _context.SaveChangesAsync();
            return Ok("Request rejected");
        }
    

        // ----------------------------------
        // ✅ Get ALL schedule for specific technician (For Dashboard)
        // ----------------------------------
        [HttpGet("GetMySchedule/{technicianId}")]
        public async Task<IActionResult> GetMySchedule(long technicianId)
        {
            var schedule = await _context.TechnicianSchedules
                .Where(s => s.TechnicianId == technicianId)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return Ok(schedule);
        }

        // ----------------------------------
        // ✅ Add a new slot
        // ----------------------------------
        [HttpPost("AddSlot")]
        public async Task<IActionResult> AddSlot([FromBody] AddSlotRequest req)
        {
            // Basic Validation
            if (req.StartTime >= req.EndTime)
                return BadRequest("Start time must be before end time.");

            if (req.Date.Date < DateTime.Now.Date)
                return BadRequest("Cannot add slots in the past.");

            var newSlot = new TechnicianSchedule
            {
                TechnicianId = req.TechnicianId,
                Date = req.Date,
                StartTime = req.StartTime,
                EndTime = req.EndTime,
                IsBooked = false
            };

            _context.TechnicianSchedules.Add(newSlot);
            await _context.SaveChangesAsync();

            return Ok(newSlot);
        }

        // ----------------------------------
        // ✅ Delete a slot
        // ----------------------------------
        [HttpDelete("DeleteSlot/{id}")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var slot = await _context.TechnicianSchedules.FindAsync(id);

            if (slot == null)
                return NotFound("Slot not found");

            if (slot.IsBooked)
                return BadRequest("Cannot delete a booked slot.");

            _context.TechnicianSchedules.Remove(slot);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }

    // Request model
    public class BookingRequest
{
    public int ScheduleId { get; set; }
    public long CustomerId { get; set; }
}

public class AddSlotRequest
{
    public long TechnicianId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
}
