using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        IRepository<Message> _messageRepository;

        public MessageController(IRepository<Message> repository)
        {
            _messageRepository = repository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var address = await _messageRepository.GetAll();
            return Ok(address);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var message = await _messageRepository.GetById(id);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Message address)
        {
            await _messageRepository.Add(address);
            return CreatedAtAction(nameof(GetById), new { id = address.Id }, address);
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
