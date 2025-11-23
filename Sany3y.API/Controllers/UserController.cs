using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        private IRepository<ProfilePicture> _pictureRepository;
        private IRepository<Address> _addressRepository;
        private UserRepository _userRepository;
        private UserManager<User> _userManager;

        public UserController(IRepository<Address> addressRepository, IRepository<ProfilePicture> pictureRepository, UserRepository repository, UserManager<User> manager)
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

<<<<<<< Updated upstream
        [HttpGet("GetTaskers")]
        public async Task<IActionResult> GetTaskers()
        {
            var users = await _userRepository.GetUsersByRole("Tasker");
            return Ok(users);
        }

        //[HttpPost("Create")]
        //public async Task<IActionResult> Create(UserDTO registerUser)
        //{
        //    var user = new User
        //    {
        //        FirstName = registerUser.FirstName,
        //        LastName = registerUser.LastName,
        //        UserName = registerUser.UserName,
        //        Email = registerUser.Email,
        //        PhoneNumber = registerUser.PhoneNumber,
        //        BirthDate = registerUser.BirthDate,
        //    };
=======
        [HttpPost("Create")]
        public async Task<IActionResult> Create(RegisterUserViewModel userDto)
        {
            var newAddress = new Address
            {
                City = userDto.City,
                Street = userDto.Street
            };
            await _addressRepository.Add(newAddress);
>>>>>>> Stashed changes

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
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
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
        public async Task<IActionResult> Update(int id, UserCreateDTO userDto)
        {
            var existingUser = await _userRepository.GetById(id);

            if (existingUser == null)
                return NotFound();

            if (existingUser.NationalId != userDto.NationalID)
                return BadRequest();

            existingUser.FirstName = userDto.FirstName;
            existingUser.LastName = userDto.LastName;
            existingUser.PhoneNumber = userDto.PhoneNumber;
            existingUser.BirthDate = userDto.BirthDate.ToDateTime(TimeOnly.MinValue);

            await _userRepository.Update(existingUser);
            return NoContent();
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
