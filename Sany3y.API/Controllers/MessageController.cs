using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private IRepository<Message> _messageRepository;
        private UserManager<User> _userManager;

        public MessageController(IRepository<Message> repository, UserManager<User> userManager)
        {
            _messageRepository = repository;
            _userManager = userManager;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var messages = await _messageRepository.GetAll();
            return Ok(messages);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var message = await _messageRepository.GetById(id);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        // في نفس الـ Controller الموجود فيه Messages/Chat
        [HttpGet("GetChatPartners/{userId}")]
        public async Task<IActionResult> GetChatPartners(long userId)
        {
            // نجيب كل الرسائل اللي تخص المستخدم
            var allMessages = await _messageRepository.GetAll();

            // نجيب كل الـ Ids المختلفين اللي اتكلم معهم المستخدم
            var partnerIds = allMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToList();

            // جلب كل المستخدمين من UserManager
            var partners = new List<User>();
            foreach (var id in partnerIds)
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user != null)
                    partners.Add(user);
            }

            return Ok(partners);
        }

        [HttpGet("GetConversation/{userId}/{otherUserId}")]
        public async Task<IActionResult> GetConversation(long userId, long otherUserId)
        {
            var allMessages = await _messageRepository.GetAll();
            var conversation = allMessages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .ToList();
            return Ok(conversation);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Message message)
        {
            await _messageRepository.Add(message);
            return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, Message message)
        {
            if (id != message.Id)
                return BadRequest();

            var existingMessage = await _messageRepository.GetById(id);
            if (existingMessage == null)
                return NotFound();

            await _messageRepository.Update(message);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _messageRepository.GetById(id);
            if (message == null)
                return NotFound();

            await _messageRepository.Delete(message);
            return NoContent();
        }
    }
}
