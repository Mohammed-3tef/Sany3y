using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sany3y.Infrastructure.Models;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        RoleManager<Role> _roleManager;

        public RoleController(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return NotFound();
            return Ok(role);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Role role)
        {
            await _roleManager.CreateAsync(role);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, Role role)
        {
            if (id != role.Id)
                return BadRequest();

            var existingRole = await _roleManager.FindByIdAsync(id.ToString());
            if (existingRole == null)
                return NotFound();

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;

            await _roleManager.UpdateAsync(role);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return NotFound();

            await _roleManager.DeleteAsync(role);
            return NoContent();
        }
    }
}
