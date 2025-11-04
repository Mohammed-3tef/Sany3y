using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        IRepository<Category> _categoryRepository;

        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepository.GetAll();
            return Ok(categories);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryRepository.GetById(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Category category)
        {
            await _categoryRepository.Add(category);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.Id)
                return BadRequest();

            var existingCategory = await _categoryRepository.GetById(id);
            if (existingCategory == null)
                return NotFound();

            await _categoryRepository.Update(category);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetById(id);
            if (category == null)
                return NotFound();

            await _categoryRepository.Delete(category);
            return Ok($"{category.Name} deleted successfully.");
        }
    }
}
