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
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IRepository<Address> addressRepository;
        private readonly IRepository<UserPhone> phoneRepository;

        public AccountController(UserManager<User> _userManager, SignInManager<User> _signInManager, IRepository<Address> _addressRepository, IRepository<UserPhone> _phoneRepository)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            addressRepository = _addressRepository;
            this.phoneRepository = _phoneRepository;
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
            IdentityResult identityResult = await userManager.CreateAsync(newUser, registerUser.Password);

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

            var user = await userManager.FindByNameAsync(loginUser.UserName)
                        ?? await userManager.FindByEmailAsync(loginUser.UserName);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "This username doesn't exist.");
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
