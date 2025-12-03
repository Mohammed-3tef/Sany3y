using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilePictureController : ControllerBase
    {
        IRepository<ProfilePicture> _profilePictureRepository;

        public ProfilePictureController(IRepository<ProfilePicture> repository)
        {
            _profilePictureRepository = repository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var pictures = await _profilePictureRepository.GetAll();
            return Ok(pictures);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var picture = await _profilePictureRepository.GetById(id);
            if (picture == null)
                return NotFound();
            return Ok(picture);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(ProfilePicture picture)
        {
            await _profilePictureRepository.Add(picture);
            return CreatedAtAction(nameof(GetById), new { id = picture.Id }, picture);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, ProfilePicture picture)
        {
            if (id != picture.Id)
                return BadRequest();

            var existingProfilePicture = await _profilePictureRepository.GetById(id);
            if (existingProfilePicture == null)
                return NotFound();

            existingProfilePicture.Path = picture.Path;
            await _profilePictureRepository.Update(existingProfilePicture);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var picture = await _profilePictureRepository.GetById(id);
            if (picture == null)
                return NotFound();

            await _profilePictureRepository.Delete(picture);
            return NoContent();
        }
    }
}
