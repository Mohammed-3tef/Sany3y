using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        IRepository<Rating> _ratingRepository;

        public RatingController(IRepository<Rating> repository)
        {
            _ratingRepository = repository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var ratings = await _ratingRepository.GetAll();
            return Ok(ratings);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rating = await _ratingRepository.GetById(id);
            if (rating == null)
                return NotFound();
            return Ok(rating);
        }

        [HttpGet("GetByTaskerId/{taskerId}")]
        public async Task<IActionResult> GetByTaskerId(long taskerId)
        {
            var allRatings = await _ratingRepository.GetAll();
            var taskerRatings = allRatings.Where(r => r.TaskerId == taskerId).ToList();
            return Ok(taskerRatings);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Rating rating)
        {
            await _ratingRepository.Add(rating);
            return CreatedAtAction(nameof(GetById), new { id = rating.Id }, rating);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, Rating rating)
        {
            if (id != rating.Id)
                return BadRequest();

            var existingRating = await _ratingRepository.GetById(id);
            if (existingRating == null)
                return NotFound();

            await _ratingRepository.Update(rating);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rating = await _ratingRepository.GetById(id);
            if (rating == null)
                return NotFound();

            await _ratingRepository.Delete(rating);
            return NoContent();
        }
    }
}
