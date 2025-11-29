using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.DTOs;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using Sany3y.Infrastructure.ViewModels;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IRepository<Address> _addressRepository;
        private IRepository<ProfilePicture> _pictureRepository;
        private UserRepository _userRepository;
        private UserManager<User> _userManager;

        public UserController(
            IRepository<Address> addressRepository,
            IRepository<ProfilePicture> pictureRepository,
            UserRepository repository,
            UserManager<User> manager)
        {
            _userRepository = repository;
            _userManager = manager;
            _addressRepository = addressRepository;
            _pictureRepository = pictureRepository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAll();
            return Ok(users);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("GetByNationalId/{nationalId}")]
        public async Task<IActionResult> GetByNationalId(long nationalId)
        {
            var user = await _userRepository.GetByNationalId(nationalId);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("GetByUserName/{userName}")]
        public async Task<IActionResult> GetByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("GetByEmail/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(RegisterUserViewModel userDto)
        {
            var newAddress = new Address
            {
                City = userDto.City,
                Street = userDto.Street,
                Governorate = userDto.Governorate,
            };
            await _addressRepository.Add(newAddress);

            var newPicture = new ProfilePicture();

            if (!string.IsNullOrEmpty(userDto.Picture))
            {
                newPicture.Path = userDto.Picture;
                await _pictureRepository.Add(newPicture);
            }

            var user = new User
            {
                NationalId = userDto.NationalId,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                UserName = userDto.UserName,
                Email = userDto.Email,
                Gender = userDto.IsMale ? 'M' : 'F',
                PhoneNumber = userDto.PhoneNumber,
                BirthDate = userDto.BirthDate,
                AddressId = newAddress.Id,
                ProfilePictureId = newPicture.Id != 0 ? newPicture.Id : null,
                CategoryID = userDto.IsClient ? null : userDto.CategoryId,
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("UpdateRole/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] string newRole)
        {
            if (string.IsNullOrWhiteSpace(newRole))
                return BadRequest("Role cannot be empty.");

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return BadRequest("Failed to remove user from current roles.");

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
                return BadRequest("Failed to add user to the new role.");

            return Ok(new { message = $"User role updated to {newRole}" });
        }


        [HttpPut("UpdateState/{id}")]
        public async Task<IActionResult> UpdateState(int id, [FromBody] bool isOnline)
        {
            var existingUser = await _userRepository.GetById(id);
            if (existingUser == null)
                return NotFound();

            existingUser.IsOnline = isOnline;
            await _userRepository.Update(existingUser);
            return Ok(existingUser);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UserUpdateDTO userDto)
        {
            var existingUser = await _userRepository.GetById(id);

            if (existingUser == null)
                return NotFound();

            if (existingUser.Id != userDto.Id)
                return BadRequest();

            existingUser.FirstName = userDto.FirstName;
            existingUser.LastName = userDto.LastName;
            existingUser.PhoneNumber = userDto.PhoneNumber;
            existingUser.BirthDate = userDto.BirthDate.ToDateTime(TimeOnly.MinValue);
            existingUser.Bio = userDto.Bio;

            await _userRepository.Update(existingUser);
            return CreatedAtAction(nameof(GetById), new { id = existingUser.Id }, existingUser);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var address = await _userRepository.GetById(id);
            if (address == null)
                return NotFound();

            await _userRepository.Delete(address);
            return NoContent();
        }
    }
}