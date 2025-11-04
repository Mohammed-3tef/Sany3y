using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Repositories;
using Task = Sany3y.Infrastructure.Models.Task;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        IRepository<Task> _taskRepository;

        public TaskController(IRepository<Task> repository)
        {
            _taskRepository = repository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskRepository.GetAll();
            return Ok(tasks);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _taskRepository.GetById(id);
            if (task == null)
                return NotFound();
            return Ok(task);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Task task)
        {
            await _taskRepository.Add(task);
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, Task task)
        {
            if (id != task.Id)
                return BadRequest();

            var existingTask = await _taskRepository.GetById(id);
            if (existingTask == null)
                return NotFound();

            await _taskRepository.Update(task);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskRepository.GetById(id);
            if (task == null)
                return NotFound();

            await _taskRepository.Delete(task);
            return NoContent();
        }
    }
}
