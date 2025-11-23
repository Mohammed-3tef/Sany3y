using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Sany3y.Hubs;
using Sany3y.Infrastructure.DTOs;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sany3y.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _http;
        private readonly IHubContext<UserStatusHub> _hubContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;

        private string GenerateJwtToken(User user, IConfiguration configuration)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, _userManager.GetRolesAsync(user).Result.FirstOrDefault() ?? "User")
            };

            // قراءة الـ Key من appsettings.json وتحويله من Base64
            var key = new SymmetricSecurityKey(
                Convert.FromBase64String(configuration["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async System.Threading.Tasks.Task UpdateUserOnlineStatus(User user, bool isOnline)
        {
            var content = JsonContent.Create(isOnline);

            var response = await _http.PutAsync($"/api/User/UpdateState/{user.Id}", content);
            if (!response.IsSuccessStatusCode)
            {
                var errors = await SafeReadErrors(response);
                foreach (var e in errors)
                    ModelState.AddModelError(string.Empty, e);
            }

            await _hubContext.Clients.All.SendAsync("ReceiveUserStatus", user.Id, isOnline);
        }

        private async Task<List<string>> SafeReadErrors(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return new List<string> { "Unknown server error." };

            try
            {
                return JsonSerializer.Deserialize<List<string>>(content)
                       ?? new List<string> { "Unknown server error." };
            }
            catch
            {
                return new List<string> { content };
            }
        }

        private async Task<bool> HandleResponseErrors(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return true; // مفيش مشكلة

            var errors = await SafeReadErrors(response);
            foreach (var e in errors)
                ModelState.AddModelError(string.Empty, e);

            return false;
        }

        /// <summary>
        /// Checks if the user’s profile is incomplete (example logic).
        /// </summary>
        private bool IsProfileIncomplete(User user)
        {
            return string.IsNullOrWhiteSpace(user.FirstName)
                || string.IsNullOrWhiteSpace(user.LastName)
                || string.IsNullOrWhiteSpace(user.PhoneNumber)
                || string.IsNullOrWhiteSpace(user.Email)
                || string.IsNullOrWhiteSpace(user.UserName)
                || user.NationalId == 0
                || user.AddressId == null || user.BirthDate == null || user.Gender == null;
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

        public AccountController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHubContext<UserStatusHub> hubContext,
            IEmailSender emailSender,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _configuration = configuration;
            _http = httpClientFactory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7178/");

            _hubContext = hubContext;
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Register", model);

            // Check if National ID is already registered
            var existingUser = await _http.GetAsync($"/api/User/GetByNationalId/{model.NationalId}");
            if (existingUser.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "This National ID is already registered.");
                return View("Register", model);
            }

            // Create Address first
            var address = new Address { City = model.City, Street = model.Street };
            var response = await _http.PostAsJsonAsync("/api/Address/Create", address);
            if (!await HandleResponseErrors(response))
            {
                return View("Register", model);
            }
            var createdAddress = await response.Content.ReadFromJsonAsync<Address>();

            // Create User
            response = await _http.PostAsJsonAsync("/api/User/Create", model);
            if (!await HandleResponseErrors(response))
            {
                await _http.DeleteAsync($"/api/Address/Delete/{createdAddress.Id}");
                return View("Register", model);
            }

            // Assign Role
            var user = await response.Content.ReadFromJsonAsync<User>();
            if (model.IsClient)
                await _userManager.AddToRoleAsync(user, "Client");
            else
                await _userManager.AddToRoleAsync(user, "Technician");

            // Send Email Confirmation
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

            // Check if username exists
            var response = await _http.GetAsync($"/api/User/GetByUsername/{model.UserName}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View("Login", model);
            }
            var user = await response.Content.ReadFromJsonAsync<User>();

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                await SendEmailConfirmationAsync(user);
                TempData["Info"] = "Please confirm your email. A new confirmation link has been sent.";
                return RedirectToAction(nameof(EmailConfirmationNotice));
            }

            // Attempt to sign in the user with the provided credentials
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View("Login", model);
            }

            // Make Account online after login
            await UpdateUserOnlineStatus(user, true);
            var token = GenerateJwtToken(user, _configuration);
            HttpContext.Session.SetString("JwtToken", token);
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

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;
            var pictureUrl = info.Principal.FindFirstValue("picture");
            var provider = info.LoginProvider;
            var providerKey = info.ProviderKey;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Your external provider did not provide an email. Please register manually.";
                return RedirectToAction(nameof(Login));
            }

            var response = await _http.GetAsync($"/api/User/GetByEmail/{email}");
            if (response.IsSuccessStatusCode)
            {
                var existingUser = await response.Content.ReadFromJsonAsync<User>();

                // Auto confirm email for external login
                if (!existingUser.EmailConfirmed)
                {
                    existingUser.EmailConfirmed = true;
                    await _userManager.UpdateAsync(existingUser);
                }

                // Link external login if not already linked
                var linkedLogins = await _userManager.GetLoginsAsync(existingUser);
                if (!linkedLogins.Any(l => l.LoginProvider == provider && l.ProviderKey == providerKey))
                    await _userManager.AddLoginAsync(existingUser, info);

                // Make Account is online after login
                await UpdateUserOnlineStatus(existingUser, true);
                await _signInManager.SignInAsync(existingUser, isPersistent: true);

                if (IsProfileIncomplete(existingUser))
                    return RedirectToAction("CompleteProfile", "Account");

                return LocalRedirect(returnUrl);
            }

            // User not registered → send data to CompleteProfile view
            var registerModel = new RegisterUserViewModel
            {
                UserName = email.Split('@')[0],
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Picture = pictureUrl,
            };

            // Redirect to CompleteProfile with prefilled data
            return View("CompleteProfile", registerModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CompleteProfile(RegisterUserViewModel model) => View(model);

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfilePost(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _http.GetAsync($"/api/User/GetByNationalId/{model.NationalId}");
            if (response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "This National ID is already registered.");
                return View(model);
            }

            // تأكد إن الإيميل لسه مش مستخدم
            response = await _http.GetAsync($"/api/User/GetByEmail/{model.Email}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Error"] = "This email is already registered.";
                return RedirectToAction(nameof(Login));
            }

            Address address = new Address
            {
                City = model.City, Street = model.Street
            };
            response = await _http.PostAsJsonAsync("/api/Address/Create", address);
            if (!await HandleResponseErrors(response))
            {
                return View(model);
            }

            // إنشاء المستخدم فعلاً
            response = await _http.PostAsJsonAsync("/api/User/Create", model);
            if (!await HandleResponseErrors(response))
            {
                return View(model);
            }
            var user = await response.Content.ReadFromJsonAsync<User>();

            // حفظ صورة البروفايل لو جت من Google
            if (!string.IsNullOrEmpty(model.Picture))
            {
                var profilePic = new ProfilePicture { Path = model.Picture };
                response = await _http.PostAsJsonAsync("/api/ProfilePicture/Create", profilePic);

                user.ProfilePictureId = profilePic.Id;
                await _userManager.UpdateAsync(user);
            }

            if (model.IsClient)
                await _userManager.AddToRoleAsync(user, "Client");
            else
                await _userManager.AddToRoleAsync(user, "Technician");

            // تسجيل الدخول مباشرة بعد الإكمال
            await _signInManager.SignInAsync(user, isPersistent: true);

            // Make Account is online after login
            await UpdateUserOnlineStatus(user, true);
            var token = GenerateJwtToken(user, _configuration);
            HttpContext.Session.SetString("JwtToken", token);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var response = await _http.GetAsync($"/api/User/GetByUsername/{User.Identity?.Name}");
            if (response.IsSuccessStatusCode)
            {
                var user = await _userManager.GetUserAsync(User);
                await UpdateUserOnlineStatus(user, false);
                HttpContext.Session.Remove("JwtToken");
                await _signInManager.SignOutAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            await _signInManager.SignOutAsync();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "Home");

            var user = await _http.GetFromJsonAsync<User>($"/api/User/GetByID/{userId}");
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

            var response = await _http.GetAsync($"/api/User/GetByEmail/{email}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "No account found with this email.");
                return View();
            }
            var user = await response.Content.ReadFromJsonAsync<User>();

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

            var response = await _http.GetAsync($"/api/User/GetByEmail/{model.Email}");
            if (!response.IsSuccessStatusCode)
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
        [Authorize]
        public IActionResult ChangePassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }

            return View(new ChangePasswordViewModel { Email = email });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }

            var response = await _http.GetAsync($"/api/User/GetByEmail/{model.Email}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }
            var user = await response.Content.ReadFromJsonAsync<User>();

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
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ProfileAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            User currentUser = await _http.GetFromJsonAsync<User>($"/api/User/GetByUsername/{User.Identity.Name}");
            UserDTO userDTO = new UserDTO()
            {
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                UserName = currentUser.UserName,
                BirthDate = currentUser.BirthDate,
                Email = currentUser.Email,
                PhoneNumber = currentUser.PhoneNumber.ToString(),
                City = _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{currentUser.AddressId}").Result?.City ?? string.Empty,
                Street = _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{currentUser.AddressId}").Result?.Street ?? string.Empty,
                Bio = currentUser.Bio
            };
            return View(userDTO);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProfile(UserDTO userDTO)
        {
            if (!ModelState.IsValid)
                return View("Profile", userDTO);

            var response = await _http.GetAsync($"/api/User/GetByUsername/{User.Identity.Name}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View("Profile", userDTO);
            }
            var currentUser = await response.Content.ReadFromJsonAsync<User>();

            Address address = new Address
            {
                Id = currentUser.AddressId,
                City = userDTO.City,
                Street = userDTO.Street
            };

            response = await _http.PutAsync($"/api/Address/Update/{address}", null);
            if (!await HandleResponseErrors(response))
            {
                return View("Profile", userDTO);
            }

            currentUser.FirstName = userDTO.FirstName;
            currentUser.LastName = userDTO.LastName;
            currentUser.Email = userDTO.Email;
            currentUser.PhoneNumber = userDTO.PhoneNumber;
            currentUser.BirthDate = userDTO.BirthDate;
            currentUser.Bio = userDTO.Bio;
            await _userManager.UpdateAsync(currentUser);

            return RedirectToAction("Index", "Home");
        }
    }
}
