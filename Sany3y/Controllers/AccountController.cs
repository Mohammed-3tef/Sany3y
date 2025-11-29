using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sany3y.Hubs;
using Sany3y.Infrastructure.DTOs;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.ViewModels;
using Sany3y.Services;
using System.Security.Claims;

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

        private readonly JwtTokenService _jwtService;
        private readonly EmailService _emailService;
        private readonly OcrService _ocrService;

        #region Helpers

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
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token },
                protocol: Request.Scheme);

            await _emailService.SendConfirmationAsync(user, callbackUrl);
        }
        
        private async System.Threading.Tasks.Task PopulateGovernoratesAsync()
        {
            var governorates = await _http.GetFromJsonAsync<List<Governorate>>("/api/CountryServices/GetAllGovernorates");
            ViewBag.AllGovernorates = governorates?.OrderBy(g => g.ArabicName).ToList();
        }
        
        private async System.Threading.Tasks.Task GetAllCategories()
        {
            ViewBag.AllCategories = await _http.GetFromJsonAsync<List<Category>>("/api/Category/GetAll");
        }

        #endregion

        public AccountController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHubContext<UserStatusHub> hubContext,
            IEmailSender emailSender,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            JwtTokenService jwtService,
            EmailService emailService,
            OcrService ocrService
            )
        {
            _configuration = configuration;
            _http = httpClientFactory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7178/");

            _hubContext = hubContext;
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;

            _jwtService = jwtService;
            _emailService = emailService;
            _ocrService = ocrService;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await GetAllCategories();
            await PopulateGovernoratesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View("Register", model);
            }

            if (model.BirthDate >= DateTime.Now)
            {
                ModelState.AddModelError("BirthDate", "تاريخ الميلاد غير صالح.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View("Register", model);
            }

            if (!model.IsClient && model.CategoryId == null)
            {
                ModelState.AddModelError("CategoryId", "يرجى اختيار فئة فني.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View("Register", model);
            }

            // تحقق من الرقم القومي باستخدام OCR
            if (model.NationalIdImage == null || model.NationalIdImage.Length == 0)
            {
                ModelState.AddModelError("NationalIdImage", "يرجى رفع صورة البطاقة.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View(model);
            }

            string extractedId = await _ocrService.DetectNationalIdAsync(model.NationalIdImage);
            if (string.IsNullOrEmpty(extractedId))
            {
                ModelState.AddModelError(string.Empty, "تعذّر قراءة الرقم القومي من الصورة.");
                await PopulateGovernoratesAsync();
                await GetAllCategories();
                return View(model);
            }
            if (extractedId != model.NationalId.ToString())
            {
                ModelState.AddModelError("NationalId", "الرقم القومي لا يطابق الصورة المرفوعة.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View("Register", model);
            }

            var userGovernorate = await _http.GetFromJsonAsync<Governorate>($"/api/CountryServices/GetGovernorateById/{model.Governorate}");
            var userCity = await _http.GetFromJsonAsync<City>($"/api/CountryServices/GetCityByID/{model.City}");

            // إرسال البيانات للـ API باستخدام MultipartFormDataContent
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model.NationalId.ToString()), "NationalId");
            form.Add(new StringContent(model.FirstName ?? ""), "FirstName");
            form.Add(new StringContent(model.LastName ?? ""), "LastName");
            form.Add(new StringContent(model.UserName ?? ""), "UserName");
            form.Add(new StringContent(model.Email ?? ""), "Email");
            form.Add(new StringContent(model.PhoneNumber ?? ""), "PhoneNumber");
            form.Add(new StringContent(model.BirthDate.ToString("yyyy-MM-dd")), "BirthDate");
            form.Add(new StringContent(model.IsMale.ToString()), "IsMale");
            form.Add(new StringContent(userGovernorate?.ArabicName ?? ""), "Governorate");
            form.Add(new StringContent(userCity?.ArabicName ?? ""), "City");
            form.Add(new StringContent(model.Street ?? ""), "Street");
            form.Add(new StringContent(model.Password ?? ""), "Password");
            form.Add(new StringContent(model.ConfirmPassword ?? ""), "ConfirmPassword");
            form.Add(new StringContent(model.IsClient.ToString()), "IsClient");
            if (!string.IsNullOrEmpty(model.CategoryId.ToString()))
                form.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

            // الملف
            if (model.NationalIdImage != null && model.NationalIdImage.Length > 0)
            {
                var stream = new StreamContent(model.NationalIdImage.OpenReadStream());
                stream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.NationalIdImage.ContentType);
                form.Add(stream, "NationalIdImage", model.NationalIdImage.FileName);
            }

            var response = await _http.PostAsync("/api/User/Create", form);

            if (!response.IsSuccessStatusCode)
            {
                // استخدام SafeReadErrors عشان نتعامل مع أي شكل من أشكال response
                var errors = await ErrorResponseHandler.SafeReadErrors(response);
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View(model);
            }

            // قراءة المستخدم الناتج
            var apiResult = await _userManager.FindByNameAsync(model.UserName);
            if (apiResult == null)
            {
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء إنشاء المستخدم عبر API.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View("Register", model);
            }

            // إضافة الدور
            if (model.IsClient)
                await _userManager.AddToRoleAsync(apiResult, "Client");
            else
                await _userManager.AddToRoleAsync(apiResult, "Technician");

            // إرسال تأكيد البريد
            await SendEmailConfirmationAsync(apiResult);
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
            await UserStatusUpdater.UpdateUserOnlineStatus(user, true, _http, _hubContext, this);
            var token = await _jwtService.GenerateTokenAsync(user);
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
            await PopulateGovernoratesAsync();
            await GetAllCategories();

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

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
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
                await UserStatusUpdater.UpdateUserOnlineStatus(existingUser, true, _http, _hubContext, this);
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
        public async Task<IActionResult> CompleteProfileAsync(RegisterUserViewModel model)
        {
            await GetAllCategories();
            await PopulateGovernoratesAsync();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfilePost(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.BirthDate >= DateTime.Now)
            {
                ModelState.AddModelError("BirthDate", "تاريخ الميلاد غير صالح.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View(model);
            }

            if (!model.IsClient && model.CategoryId == null)
            {
                ModelState.AddModelError("CategoryId", "يرجى اختيار فئة فني.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View(model);
            }

            var response = await _http.GetAsync($"/api/User/GetByNationalId/{model.NationalId}");
            if (response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("NationalId", "هذا الرقم القومي مسجل بالفعل.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View(model);
            }

            // تأكد إن الإيميل لسه مش مستخدم
            response = await _http.GetAsync($"/api/User/GetByEmail/{model.Email}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Error"] = "This email is already registered.";
                return RedirectToAction(nameof(Login));
            }

            // Using OCR to Check the National ID
            if (model.NationalIdImage == null || model.NationalIdImage.Length == 0)
            {
                ModelState.AddModelError("NationalIdImage", "يرجى رفع صورة البطاقة.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View(model);
            }
            string extractedId = await _ocrService.DetectNationalIdAsync(model.NationalIdImage);

            if (string.IsNullOrEmpty(extractedId))
            {
                ModelState.AddModelError(string.Empty, "تعذّر قراءة الرقم القومي من الصورة.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View("Register", model);
            }

            if (extractedId != model.NationalId.ToString())
            {
                ModelState.AddModelError("NationalId", "الرقم القومي لا يطابق الصورة المرفوعة.");
                await GetAllCategories();
                await PopulateGovernoratesAsync();
                return View("Register", model);
            }

            var userGovernorate = await _http.GetFromJsonAsync<Governorate>($"/api/CountryServices/GetGovernorateById/{model.Governorate}");
            var userCity = await _http.GetFromJsonAsync<City>($"/api/CountryServices/GetCityByID/{model.City}");

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model.NationalId.ToString()), "NationalId");
            form.Add(new StringContent(model.FirstName ?? ""), "FirstName");
            form.Add(new StringContent(model.LastName ?? ""), "LastName");
            form.Add(new StringContent(model.UserName ?? ""), "UserName");
            form.Add(new StringContent(model.Email ?? ""), "Email");
            form.Add(new StringContent(model.PhoneNumber ?? ""), "PhoneNumber");
            form.Add(new StringContent(model.BirthDate.ToString("yyyy-MM-dd")), "BirthDate");
            form.Add(new StringContent(model.IsMale.ToString()), "IsMale");
            form.Add(new StringContent(userGovernorate?.ArabicName ?? ""), "Governorate");
            form.Add(new StringContent(userCity?.ArabicName ?? ""), "City");
            form.Add(new StringContent(model.Street ?? ""), "Street");
            form.Add(new StringContent(model.Password ?? ""), "Password");
            form.Add(new StringContent(model.ConfirmPassword ?? ""), "ConfirmPassword");
            form.Add(new StringContent(model.IsClient.ToString()), "IsClient");
            if (!string.IsNullOrEmpty(model.CategoryId.ToString()))
                form.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

            // الملف
            if (model.NationalIdImage != null && model.NationalIdImage.Length > 0)
            {
                var stream = new StreamContent(model.NationalIdImage.OpenReadStream());
                stream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.NationalIdImage.ContentType);
                form.Add(stream, "NationalIdImage", model.NationalIdImage.FileName);
            }

            response = await _http.PostAsync("/api/User/Create", form);
            if (!await ErrorResponseHandler.HandleResponseErrors(response, ModelState))
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.UserName);

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
            await UserStatusUpdater.UpdateUserOnlineStatus(user, true, _http, _hubContext, this);
            var token = await _jwtService.GenerateTokenAsync(user);
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
                await UserStatusUpdater.UpdateUserOnlineStatus(user, false, _http, _hubContext, this);
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
                $@"
                <!DOCTYPE html>
                <html lang='ar' dir='rtl'>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 0 20px rgba(0,0,0,0.05); }}
                        .header {{ background: linear-gradient(135deg, #198754, #006400); padding: 30px; text-align: center; color: #ffffff; }}
                        .header h1 {{ margin: 0; font-size: 28px; font-weight: 800; letter-spacing: -1px; }}
                        .header i {{ font-size: 24px; margin-left: 10px; color: #ffc107; }}
                        .content {{ padding: 40px 30px; color: #333333; line-height: 1.8; text-align: right; }}
                        .welcome-text {{ font-size: 20px; font-weight: 600; color: #198754; margin-bottom: 20px; }}
                        .button-container {{ text-align: center; margin: 30px 0; }}
                        .button {{ display: inline-block; padding: 15px 40px; background-color: #ffc107; color: #000000; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 16px; transition: all 0.3s ease; box-shadow: 0 4px 15px rgba(255,193,7,0.3); }}
                        .button:hover {{ transform: translateY(-2px); box-shadow: 0 6px 20px rgba(255,193,7,0.4); background-color: #ffca2c; }}
                        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 13px; color: #6c757d; border-top: 1px solid #eee; }}
                        .social-links {{ margin-top: 10px; }}
                        .social-links a {{ color: #6c757d; margin: 0 5px; text-decoration: none; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1><span style='color: #ffc107;'>✦</span> Sany3y | صنايعي</h1>
                        </div>

                        <div class='content'>
                            <div class='welcome-text'>طلب تغيير كلمة المرور 🔐</div>

                            <p>لقد استلمنا طلباً لتغيير كلمة المرور الخاصة بحسابك على منصة <strong>صنايعي</strong>.</p>
                            <p>لإتمام العملية يرجى الضغط على الزر أدناه لتغيير كلمة المرور الخاصة بك:</p>

                            <div class='button-container'>
                                <a href='{Url.Action("ChangePassword", "Account", new { email = model.Email }, Request.Scheme)}' 
                                   class='button'>
                                    تغيير كلمة المرور
                                </a>
                            </div>

                            <p style='margin-top: 30px; font-size: 14px; color: #999; border-top: 1px solid #eee; padding-top: 20px;'>
                                إذا لم تقم بهذا الطلب، يمكنك تجاهل هذا البريد ولن يتم تغيير كلمة المرور الخاصة بك.
                            </p>
                        </div>

                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} Sany3y. جميع الحقوق محفوظة.</p>
                            <div class='social-links'>
                                <a href='#'>سياسة الخصوصية</a> | 
                                <a href='#'>شروط الاستخدام</a> | 
                                <a href='#'>تواصل معنا</a>
                            </div>
                        </div>
                    </div>
                </body>
                </html>
            "
            );

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
                Governorate = _http.GetFromJsonAsync<Address>($"/api/Address/GetByID/{currentUser.AddressId}").Result?.Governorate ?? string.Empty,
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

            var response = await _http.GetAsync($"/api/User/GetByUsername/{User?.Identity?.Name}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View("Profile", userDTO);
            }
            var currentUser = await response.Content.ReadFromJsonAsync<User>();

            Address address = new Address
            {
                Id = currentUser.AddressId,
                Governorate = userDTO.Governorate,
                City = userDTO.City,
                Street = userDTO.Street
            };

            response = await _http.PutAsync($"/api/Address/Update/{address}", null);
            if (!await ErrorResponseHandler.HandleResponseErrors(response, ModelState))
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
