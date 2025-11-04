using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        IRepository<Address> _addressRepository;

        public AddressController(IRepository<Address> repository)
        {
            _addressRepository = repository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var addresses = await _addressRepository.GetAll();
            return Ok(addresses);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var address = await _addressRepository.GetById(id);
            if (address == null)
                return NotFound();
            return Ok(address);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Address address)
        {
            await _addressRepository.Add(address);
            return CreatedAtAction(nameof(GetById), new { id = address.Id }, address);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, Address address)
        {
            if (id != address.Id)
                return BadRequest();

            var existingAddress = await _addressRepository.GetById(id);
            if (existingAddress == null)
                return NotFound();

            await _addressRepository.Update(address);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var address = await _addressRepository.GetById(id);
            if (address == null)
                return NotFound();

            await _addressRepository.Delete(address);
            return NoContent();
        }
    }
}
