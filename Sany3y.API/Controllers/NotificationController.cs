using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        IRepository<Notification> _notificationRepository;

        public NotificationController(IRepository<Notification> repository)
        {
            _notificationRepository = repository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _notificationRepository.GetAll();
            return Ok(notifications);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var notification = await _notificationRepository.GetById(id);
            if (notification == null)
                return NotFound();
            return Ok(notification);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Notification notification)
        {
            await _notificationRepository.Add(notification);
            return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, Notification notification)
        {
            if (id != notification.Id)
                return BadRequest();

            var existingNotification = await _notificationRepository.GetById(id);
            if (existingNotification == null)
                return NotFound();

            await _notificationRepository.Update(notification);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _notificationRepository.GetById(id);
            if (notification == null)
                return NotFound();

            await _notificationRepository.Delete(notification);
            return NoContent();
        }
    }
}
