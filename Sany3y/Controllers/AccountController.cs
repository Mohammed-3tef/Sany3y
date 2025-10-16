using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using Sany3y.Infrastructure.ViewModel;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Sany3y.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly UserRepository _userRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<UserPhone> _phoneRepository;
        private readonly IEmailSender _emailSender;

        public AccountController(
            IEmailSender emailSender,
            UserManager<User> userManager,
            UserRepository userRepository,
            SignInManager<User> signInManager,
            IRepository<Address> addressRepository,
            IRepository<UserPhone> phoneRepository)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _userRepository = userRepository;
            _signInManager = signInManager;
            _addressRepository = addressRepository;
            _phoneRepository = phoneRepository;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRegister(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Register", model);

            // Check for duplicate National ID
            if (await _userRepository.GetByNationalId(model.NationalId) != null)
            {
                ModelState.AddModelError("", "This National ID is already registered.");
                return View("Register", model);
            }

            // Create and save address
            var address = new Address { City = model.City, Street = model.Street };
            await _addressRepository.Add(address);

            // Create user
            var user = new User
            {
                NationalId = model.NationalId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                BirthDate = model.BirthDate,
                AddressId = address.Id
            };

            var result = await _userRepository.Add(user);

            if (!result.Succeeded)
            {
                await _addressRepository.Delete(address);
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("Register", model);
            }

            // Save phone
            await _phoneRepository.Add(new UserPhone
            {
                UserId = user.Id,
                PhoneNumber = model.PhoneNumber
            });

            // Send confirmation email
            await SendEmailConfirmationAsync(user);

            // Auto sign-in only if confirmation is not required
            if (!_userManager.Options.SignIn.RequireConfirmedAccount)
                await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["Success"] = "Account created successfully. Please confirm your email before logging in.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"User with ID '{userId}' was not found.");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            ViewBag.Message = result.Succeeded
                ? "Your email has been successfully confirmed! You can now log in."
                : "Email confirmation failed. The link may be invalid or expired.";

            return View("ConfirmEmail");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLogin(LoginUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Login", model);

            var user = await _userRepository.GetByUsername(model.UserName)
                        ?? await _userRepository.GetByEmail(model.UserName);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View("Login", model);
            }

            if (!user.EmailConfirmed)
            {
                await SendEmailConfirmationAsync(user);
                TempData["Info"] = "Please confirm your email. A new confirmation link has been sent.";
                return RedirectToAction(nameof(EmailConfirmationNotice));
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View("Login", model);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ResendConfirmation() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmation(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Please enter your email.");
                return View();
            }

            var user = await _userRepository.GetByEmail(email);
            if (user == null)
            {
                ModelState.AddModelError("", "No account found with this email.");
                return View();
            }

            if (user.EmailConfirmed)
            {
                TempData["Info"] = "This email is already confirmed.";
                return RedirectToAction(nameof(Login));
            }

            await SendEmailConfirmationAsync(user);
            TempData["Success"] = "A new confirmation link has been sent.";
            return RedirectToAction(nameof(EmailConfirmationNotice));
        }

        [HttpGet]
        public IActionResult EmailConfirmationNotice()
        {
            ViewBag.Message = TempData["Info"] ?? TempData["Success"];
            return View();
        }

        private async System.Threading.Tasks.Task SendEmailConfirmationAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { userId = user.Id, token },
                protocol: Request.Scheme);

            var message = $@"
                <h2>Welcome to Sany3y!</h2>
                <p>Click below to confirm your email:</p>
                <p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Confirm Email</a></p>";

            await _emailSender.SendEmailAsync(user.Email, "Confirm your account", message);
        }
    }
}
