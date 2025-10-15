using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using Sany3y.Infrastructure.ViewModel;

namespace Sany3y.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository userRepository;
        private readonly SignInManager<User> signInManager;
        private readonly IRepository<Address> addressRepository;
        private readonly IRepository<UserPhone> phoneRepository;

        public AccountController(UserRepository _userRepository, SignInManager<User> _signInManager, IRepository<Address> _addressRepository, IRepository<UserPhone> _phoneRepository)
        {
            userRepository = _userRepository;
            signInManager = _signInManager;
            addressRepository = _addressRepository;
            phoneRepository = _phoneRepository;
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SaveRegister(RegisterUserViewModel registerUser)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", registerUser);
            }

            bool isFoundByNationalId = await userRepository.GetByNationalId(registerUser.NationalId) != null;

            if (isFoundByNationalId)
            {
                ModelState.AddModelError(string.Empty, "This National ID is already registered.");
                return View("Register", registerUser);
            }

            Address newAddress = new Address
            {
                City = registerUser.City,
                Street = registerUser.Street
            };
            await addressRepository.Add(newAddress);

            User newUser = new User
            {
                NationalId = registerUser.NationalId,
                FirstName = registerUser.FirstName,
                LastName = registerUser.LastName,
                UserName = registerUser.UserName,
                Email = registerUser.Email,
                BirthDate = registerUser.BirthDate,
                PasswordHash = registerUser.Password,
                AddressId = newAddress.Id,
            };
            IdentityResult identityResult = await userRepository.Add(newUser);

            if (!identityResult.Succeeded)
            {
                await addressRepository.Delete(newAddress);

                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Register", registerUser);
            }

            UserPhone phone = new UserPhone
            {
                UserId = newUser.Id,
                PhoneNumber = registerUser.PhoneNumber
            };
            await phoneRepository.Add(phone);

            await signInManager.SignInAsync(newUser, false);
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> SaveLogin(LoginUserViewModel loginUser)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", loginUser);
            }

            var user = await userRepository.GetByUsername(loginUser.UserName)
                        ?? await userRepository.GetByEmail(loginUser.UserName);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View("Login", loginUser);
            }

            var result = await signInManager.PasswordSignInAsync(user, loginUser.Password, loginUser.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View("Login", loginUser);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
