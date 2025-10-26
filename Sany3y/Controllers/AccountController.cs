using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using Sany3y.Infrastructure.ViewModels;

namespace Sany3y.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly UserRepository _userRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<UserPhone> _phoneRepository;
        private readonly IRepository<ProfilePicture> _profilePictureRepo;
        private readonly IEmailSender _emailSender;

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

        public AccountController(
            IEmailSender emailSender,
            UserManager<User> userManager,
            UserRepository userRepository,
            SignInManager<User> signInManager,
            IRepository<Address> addressRepository,
            IRepository<ProfilePicture> profilePictureRepo,
            IRepository<UserPhone> phoneRepository)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _userRepository = userRepository;
            _signInManager = signInManager;
            _addressRepository = addressRepository;
            _profilePictureRepo = profilePictureRepo;
            _phoneRepository = phoneRepository;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Register", model);

            if (await _userRepository.GetByNationalId(model.NationalId) != null)
            {
                ModelState.AddModelError(string.Empty, "This National ID is already registered.");
                return View("Register", model);
            }

            var address = new Address { City = model.City, Street = model.Street };
            await _addressRepository.Add(address);

            var user = new User
            {
                NationalId = model.NationalId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                BirthDate = model.BirthDate,
                Gender = model.IsMale ? 'M' : 'F',
                PasswordHash = model.Password,
                AddressId = address.Id
            };

            var result = await _userRepository.Add(user);

            if (!result.Succeeded)
            {
                await _addressRepository.Delete(address);
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View("Register", model);
            }

            await _phoneRepository.Add(new UserPhone
            {
                UserId = user.Id,
                PhoneNumber = model.PhoneNumber
            });

            if (model.IsClient)
                await _userManager.AddToRoleAsync(user, "Client");
            else
                await _userManager.AddToRoleAsync(user, "Tasker");

            await SendEmailConfirmationAsync(user);
            return RedirectToAction(nameof(EmailConfirmationNotice));
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Login", model);

            var user = await _userRepository.GetByUsername(model.UserName);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
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
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View("Login", model);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            if (provider == "Google")
                properties.Items["prompt"] = "select_account"; // يجبر Google يفتح صفحة اختيار الإيميل

            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                TempData["Error"] = $"External provider error: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "Unable to load external login information.";
                return RedirectToAction(nameof(Login));
            }

            // Try to sign in existing user by their external login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                // Already linked Google account found
                var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                if (existingUser != null)
                {
                    if (existingUser.EmailConfirmed)
                        return LocalRedirect(returnUrl);

                    // Existing user but email not confirmed
                    TempData["Info"] = "Please confirm your email to complete sign-in.";
                    return RedirectToAction(nameof(EmailConfirmationNotice));
                }
            }

            // Get data from Google claims
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
            var pictureUrl = info.Principal.FindFirstValue("picture");

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Google did not provide an email. Please use manual registration.";
                return RedirectToAction(nameof(Login));
            }

            // Check if this email already exists in your database
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // NEW Google user → auto confirm and sign in
                var address = new Address { City = "Cairo", Street = "." };
                await _addressRepository.Add(address);

                user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName ?? " ",
                    LastName = lastName ?? " ",
                    AddressId = address.Id,
                    Gender = 'M',
                    EmailConfirmed = true
                };

                var identityResult = await _userManager.CreateAsync(user);
                if (!identityResult.Succeeded)
                {
                    foreach (var error in identityResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return View("Login");
                }

                // Assign "Client" role by default
                await _userManager.AddToRoleAsync(user, "Client");

                // Optional: Save profile picture
                if (!string.IsNullOrEmpty(pictureUrl))
                {
                    var profilePicture = new ProfilePicture { Path = pictureUrl };
                    await _profilePictureRepo.Add(profilePicture);

                    user.ProfilePictureId = profilePicture.Id;
                    await _userManager.UpdateAsync(user);
                }

                // Link Google login to new user
                await _userManager.AddLoginAsync(user, info);

                // Directly sign in (since email is confirmed)
                await _signInManager.SignInAsync(user, isPersistent: true);

                return LocalRedirect(returnUrl);
            }
            else
            {
                // Existing account found but not confirmed
                if (!user.EmailConfirmed)
                {
                    TempData["Info"] = "Please confirm your email to complete sign-in.";
                    return RedirectToAction(nameof(EmailConfirmationNotice));
                }

                // Existing account already confirmed → just link Google and sign in
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: true);

                return LocalRedirect(returnUrl);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            await _signInManager.SignOutAsync();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"User with ID '{userId}' was not found.");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            ViewBag.Message = result.Succeeded
                ? "Your email has been successfully confirmed! You can now log in."
                : "Email confirmation failed. The link may be invalid or expired.";

            await _signInManager.SignOutAsync();
            return View("ConfirmEmail");
        }

        [HttpGet]
        public IActionResult ResendConfirmation() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmation(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Please enter your email.");
                return View();
            }

            var user = await _userRepository.GetByEmail(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No account found with this email.");
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

        [HttpGet]
        public IActionResult VerifyEmail() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            await _emailSender.SendEmailAsync(
                model.Email,
                "Password Change Request",
                $"You requested to change your password. Click the link below to proceed:<br>" +
                $"<a href='{Url.Action("ChangePassword", "Account", new { email = model.Email }, Request.Scheme)}'>Change Password</a>");

            TempData["Info"] = "A password change link has been sent to your email.";

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }

            return View(new ChangePasswordViewModel { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            var result = await _userManager.RemovePasswordAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }
    }
}
