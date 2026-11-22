using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sany3y.API.Hubs;
using Sany3y.API.Services;
using Sany3y.API.Services.CountryServices;
using Sany3y.Infrastructure.DTOs;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using Sany3y.Infrastructure.ViewModels;
using System.Security.Claims;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<ProfilePicture> _pictureRepository;
        private readonly GovernorateServices _governorateServices;
        private readonly CityServices _cityServices;
        private readonly ApiJwtTokenService _jwtService;
        private readonly ApiEmailService _emailService;
        private readonly ApiOcrService _ocrService;
        private readonly IHubContext<UserStatusHub> _hubContext;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            AppDbContext context,
            IRepository<Address> addressRepository,
            IRepository<ProfilePicture> pictureRepository,
            GovernorateServices governorateServices,
            CityServices cityServices,
            ApiJwtTokenService jwtService,
            ApiEmailService emailService,
            ApiOcrService ocrService,
            IHubContext<UserStatusHub> hubContext,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _addressRepository = addressRepository;
            _pictureRepository = pictureRepository;
            _governorateServices = governorateServices;
            _cityServices = cityServices;
            _jwtService = jwtService;
            _emailService = emailService;
            _ocrService = ocrService;
            _hubContext = hubContext;
            _config = config;
        }

        private string GetFrontendUrl() => (_config["FrontendUrl"] ?? "http://localhost:3000").TrimEnd('/');

        private bool IsProfileIncomplete(User user)
        {
            return string.IsNullOrWhiteSpace(user.FirstName)
                || string.IsNullOrWhiteSpace(user.LastName)
                || string.IsNullOrWhiteSpace(user.PhoneNumber)
                || string.IsNullOrWhiteSpace(user.Email)
                || string.IsNullOrWhiteSpace(user.UserName)
                || user.NationalId == 0
                || user.AddressId == 0;
        }

        private async Task<string> ResolveGovernorateNameAsync(string govInput)
        {
            if (int.TryParse(govInput, out int govId))
            {
                var gov = await _governorateServices.GetByID(govId);
                return gov?.ArabicName ?? govInput;
            }
            return govInput ?? "";
        }

        private async Task<string> ResolveCityNameAsync(string cityInput)
        {
            if (int.TryParse(cityInput, out int cityId))
            {
                var city = await _cityServices.GetByID(cityId);
                return city?.ArabicName ?? cityInput;
            }
            return cityInput ?? "";
        }

        private async System.Threading.Tasks.Task SendEmailConfirmationLinkAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var callbackUrl = $"{GetFrontendUrl()}/confirm-email?userId={user.Id}&token={encodedToken}";
            await _emailService.SendConfirmationAsync(user, callbackUrl);
        }

        private async Task<object> BuildAuthUserResponseAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var address = user.Address ?? (user.AddressId > 0 ? await _addressRepository.GetById(user.AddressId) : null);
            var picture = user.ProfilePicture ?? (user.ProfilePictureId.HasValue ? await _pictureRepository.GetById((int)user.ProfilePictureId.Value) : null);

            return new
            {
                id = user.Id,
                userName = user.UserName,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phoneNumber = user.PhoneNumber,
                nationalId = user.NationalId,
                gender = user.Gender,
                birthDate = user.BirthDate,
                bio = user.Bio,
                isOnline = user.IsOnline,
                isShop = user.IsShop == true,
                shopName = user.ShopName,
                categoryId = user.CategoryID,
                experienceYears = user.ExperienceYears,
                price = user.Price,
                rating = user.Rating,
                roles = roles,
                role = roles.FirstOrDefault() ?? "Client",
                profilePictureUrl = picture?.Path,
                address = address != null ? new
                {
                    id = address.Id,
                    governorate = address.Governorate,
                    city = address.City,
                    street = address.Street
                } : null
            };
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "يرجى إدخال اسم المستخدم وكلمة المرور." });

            var user = await _userManager.FindByNameAsync(model.UserName)
                       ?? await _userManager.FindByEmailAsync(model.UserName);

            if (user == null)
                return Unauthorized(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة." });

            if (!user.EmailConfirmed)
            {
                try
                {
                    await SendEmailConfirmationLinkAsync(user);
                }
                catch
                {
                    // Ignore SMTP failures in dev if unconfigured
                }
                return StatusCode(403, new
                {
                    requiresEmailConfirmation = true,
                    email = user.Email,
                    message = "يرجى تأكيد بريدك الإلكتروني. تم إرسال رابط تأكيد جديد."
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return StatusCode(423, new
                {
                    isLockedOut = true,
                    message = "تم قفل الحساب مؤقتاً بسبب محاولات تسجيل دخول متعددة فاشلة. يرجى المحاولة بعد 15 دقيقة."
                });
            }

            if (!result.Succeeded)
                return Unauthorized(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة." });

            user.IsOnline = true;
            await _userManager.UpdateAsync(user);
            await _hubContext.Clients.All.SendAsync("ReceiveUserStatus", user.Id, true);

            var token = await _jwtService.GenerateTokenAsync(user);
            var userInfo = await BuildAuthUserResponseAsync(user);

            return Ok(new
            {
                token,
                user = userInfo
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterUserViewModel model)
        {
            var errors = new List<string>();

            if (model.BirthDate >= DateTime.Now)
            {
                errors.Add(model.IsShop == true ? "تاريخ الإنشاء غير صالح." : "تاريخ الميلاد غير صالح.");
            }

            if (!model.IsClient && model.CategoryId == null)
            {
                errors.Add(model.IsShop == true ? "يرجى اختيار فئة المتجر." : "يرجى اختيار فئة فني.");
            }

            if (!model.IsClient && model.IsShop != true && model.ExperienceYears == null)
            {
                errors.Add("يرجى إدخال سنوات الخبرة.");
            }

            if (!model.IsClient && model.IsShop != true && model.Price == null)
            {
                errors.Add("يرجى إدخال سعر الخدمة.");
            }

            if (model.NationalIdImage == null || model.NationalIdImage.Length == 0)
            {
                errors.Add("يرجى رفع صورة البطاقة.");
            }

            if (errors.Any())
                return BadRequest(new { errors, message = errors.First() });

            // Check National ID uniqueness
            var existingByNatId = await _context.Users.FirstOrDefaultAsync(u => u.NationalId == model.NationalId);
            if (existingByNatId != null)
            {
                return BadRequest(new { errors = new[] { "هذا الرقم القومي مسجل بالفعل." }, message = "هذا الرقم القومي مسجل بالفعل." });
            }

            // OCR Verification
            if (model.NationalIdImage != null)
            {
                try
                {
                    var extractedId = await _ocrService.DetectNationalIdAsync(model.NationalIdImage);
                    if (string.IsNullOrEmpty(extractedId))
                    {
                        return BadRequest(new { errors = new[] { "تعذّر قراءة الرقم القومي من الصورة." }, message = "تعذّر قراءة الرقم القومي من الصورة." });
                    }
                    if (extractedId != model.NationalId.ToString())
                    {
                        return BadRequest(new { errors = new[] { "الرقم القومي لا يطابق الصورة المرفوعة." }, message = "الرقم القومي لا يطابق الصورة المرفوعة." });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { errors = new[] { $"فشل التحقق من صورة البطاقة (تأكد من تشغيل خدمة الذكاء الاصطناعي OCR): {ex.Message}" }, message = "تعذّر الاتصال بخدمة قراءة البطاقة الشخصية." });
                }
            }

            var govName = await ResolveGovernorateNameAsync(model.Governorate);
            var cityName = await ResolveCityNameAsync(model.City);

            var newAddress = new Address
            {
                Governorate = govName,
                City = cityName,
                Street = model.Street
            };
            await _addressRepository.Add(newAddress);

            ProfilePicture? newPicture = null;
            if (!string.IsNullOrEmpty(model.Picture))
            {
                newPicture = new ProfilePicture { Path = model.Picture };
                await _pictureRepository.Add(newPicture);
            }

            var user = new User
            {
                NationalId = model.NationalId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                Gender = model.IsMale ? 'M' : 'F',
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate,
                AddressId = newAddress.Id,
                ProfilePictureId = newPicture?.Id,
                CategoryID = model.IsClient ? null : model.CategoryId,
                ExperienceYears = (model.IsClient || model.IsShop == true) ? null : model.ExperienceYears,
                Price = (model.IsClient || model.IsShop == true) ? null : model.Price,
                IsShop = model.IsShop,
                ShopName = model.IsShop == true ? model.ShopName : null
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                var identityErrors = createResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { errors = identityErrors, message = identityErrors.FirstOrDefault() ?? "فشل إنشاء الحساب." });
            }

            await _userManager.AddToRoleAsync(user, model.IsClient ? "Client" : "Technician");

            try
            {
                await SendEmailConfirmationLinkAsync(user);
            }
            catch
            {
                // Ignore email error in dev environment if SMTP fails
            }

            return Ok(new
            {
                message = "تم إنشاء الحساب بنجاح! يرجى مراجعة بريدك الإلكتروني لتفعيل الحساب.",
                userId = user.Id,
                requiresEmailConfirmation = true
            });
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { message = "مزود الخدمة الخارجي لم يوفر بريداً إلكترونياً." });

            var existingUser = await _userManager.FindByEmailAsync(req.Email);
            if (existingUser != null)
            {
                if (!existingUser.EmailConfirmed)
                {
                    existingUser.EmailConfirmed = true;
                    await _userManager.UpdateAsync(existingUser);
                }

                existingUser.IsOnline = true;
                await _userManager.UpdateAsync(existingUser);
                await _hubContext.Clients.All.SendAsync("ReceiveUserStatus", existingUser.Id, true);

                var token = await _jwtService.GenerateTokenAsync(existingUser);
                var userInfo = await BuildAuthUserResponseAsync(existingUser);

                return Ok(new
                {
                    isNewUser = false,
                    isProfileIncomplete = IsProfileIncomplete(existingUser),
                    token,
                    user = userInfo
                });
            }

            return Ok(new
            {
                isNewUser = true,
                prefill = new
                {
                    userName = req.Email.Split('@')[0],
                    email = req.Email,
                    firstName = req.FirstName ?? "",
                    lastName = req.LastName ?? "",
                    picture = req.PictureUrl ?? "",
                    isMale = true,
                    isClient = true
                }
            });
        }

        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromForm] RegisterUserViewModel model)
        {
            var errors = new List<string>();

            if (model.BirthDate >= DateTime.Now)
                errors.Add("تاريخ الميلاد غير صالح.");

            if (!model.IsClient && model.CategoryId == null)
                errors.Add("يرجى اختيار فئة فني.");

            if (!model.IsClient && model.ExperienceYears == null)
                errors.Add("يرجى إدخال سنوات الخبرة.");

            if (!model.IsClient && model.Price == null)
                errors.Add("يرجى إدخال سعر الخدمة.");

            if (model.NationalIdImage == null || model.NationalIdImage.Length == 0)
                errors.Add("يرجى رفع صورة البطاقة.");

            if (errors.Any())
                return BadRequest(new { errors, message = errors.First() });

            var existingNatId = await _context.Users.FirstOrDefaultAsync(u => u.NationalId == model.NationalId);
            if (existingNatId != null)
                return BadRequest(new { errors = new[] { "هذا الرقم القومي مسجل بالفعل." }, message = "هذا الرقم القومي مسجل بالفعل." });

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
                return BadRequest(new { errors = new[] { "هذا البريد الإلكتروني مسجل بالفعل." }, message = "هذا البريد الإلكتروني مسجل بالفعل." });

            try
            {
                var extractedId = await _ocrService.DetectNationalIdAsync(model.NationalIdImage!);
                if (string.IsNullOrEmpty(extractedId))
                    return BadRequest(new { errors = new[] { "تعذّر قراءة الرقم القومي من الصورة." }, message = "تعذّر قراءة الرقم القومي من الصورة." });

                if (extractedId != model.NationalId.ToString())
                    return BadRequest(new { errors = new[] { "الرقم القومي لا يطابق الصورة المرفوعة." }, message = "الرقم القومي لا يطابق الصورة المرفوعة." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"تعذّر قراءة الرقم القومي من الصورة: {ex.Message}" }, message = "تعذّر قراءة الرقم القومي من الصورة." });
            }

            var govName = await ResolveGovernorateNameAsync(model.Governorate);
            var cityName = await ResolveCityNameAsync(model.City);

            var newAddress = new Address
            {
                Governorate = govName,
                City = cityName,
                Street = model.Street
            };
            await _addressRepository.Add(newAddress);

            ProfilePicture? profilePic = null;
            if (!string.IsNullOrEmpty(model.Picture))
            {
                profilePic = new ProfilePicture { Path = model.Picture };
                await _pictureRepository.Add(profilePic);
            }

            var user = new User
            {
                NationalId = model.NationalId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true,
                Gender = model.IsMale ? 'M' : 'F',
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate,
                AddressId = newAddress.Id,
                ProfilePictureId = profilePic?.Id,
                CategoryID = model.IsClient ? null : model.CategoryId,
                ExperienceYears = model.IsClient ? null : model.ExperienceYears,
                Price = model.IsClient ? null : model.Price,
                IsOnline = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                var identityErrors = createResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { errors = identityErrors, message = identityErrors.FirstOrDefault() });
            }

            await _userManager.AddToRoleAsync(user, model.IsClient ? "Client" : "Technician");
            await _hubContext.Clients.All.SendAsync("ReceiveUserStatus", user.Id, true);

            var token = await _jwtService.GenerateTokenAsync(user);
            var userInfo = await BuildAuthUserResponseAsync(user);

            return Ok(new { token, user = userInfo });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(new { state = "Failed", message = "رابط التأكيد غير مكتمل." });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { state = "Failed", message = "المستخدم غير موجود." });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    state = "Success",
                    message = "تم تأكيد البريد الإلكتروني بنجاح! يمكنك الآن تسجيل الدخول."
                });
            }

            return BadRequest(new
            {
                state = "Failed",
                message = "فشل تأكيد البريد الإلكتروني. قد يكون الرابط غير صالح أو منتهي الصلاحية."
            });
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] VerifyEmailViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest(new { message = "يرجى إدخال البريد الإلكتروني." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new { message = "لا يوجد حساب مسجل بهذا البريد الإلكتروني." });

            if (user.EmailConfirmed)
                return Ok(new { alreadyConfirmed = true, message = "هذا البريد الإلكتروني تم تأكيده بالفعل." });

            await SendEmailConfirmationLinkAsync(user);
            return Ok(new { message = "تم إرسال رابط تأكيد جديد إلى بريدك الإلكتروني." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "يرجى إدخال بريد إلكتروني صحيح." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new { message = "لم يتم العثور على مستخدم بهذا البريد الإلكتروني." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(model.Email);
            var resetUrl = $"{GetFrontendUrl()}/reset-password?email={encodedEmail}&token={encodedToken}";

            await _emailService.SendPasswordResetAsync(model.Email, resetUrl);

            return Ok(new { message = "تم إرسال رابط تغيير كلمة المرور إلى بريدك الإلكتروني." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest(new { message = "يرجى إدخال البريد الإلكتروني وكلمة المرور الجديدة." });

            if (model.NewPassword != model.ConfirmNewPassword)
                return BadRequest(new { message = "كلمة المرور وتأكيد كلمة المرور غير متطابقين." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود." });

            IdentityResult result;
            if (!string.IsNullOrWhiteSpace(model.Token))
            {
                result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            }
            else
            {
                var removeRes = await _userManager.RemovePasswordAsync(user);
                if (!removeRes.Succeeded)
                    return BadRequest(new { errors = removeRes.Errors.Select(e => e.Description), message = "فشل تغيير كلمة المرور." });
                result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            }

            if (!result.Succeeded)
            {
                var errs = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { errors = errs, message = errs.FirstOrDefault() ?? "فشل تغيير كلمة المرور." });
            }

            return Ok(new { message = "تم تغيير كلمة المرور بنجاح!" });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;

            User? user = null;
            if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out long uid))
            {
                user = await _context.Users
                    .Include(u => u.Address)
                    .Include(u => u.ProfilePicture)
                    .Include(u => u.Category)
                    .FirstOrDefaultAsync(u => u.Id == uid);
            }
            if (user == null && !string.IsNullOrEmpty(userName))
            {
                user = await _context.Users
                    .Include(u => u.Address)
                    .Include(u => u.ProfilePicture)
                    .Include(u => u.Category)
                    .FirstOrDefaultAsync(u => u.UserName == userName);
            }

            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود." });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                id = user.Id,
                nationalId = user.NationalId,
                firstName = user.FirstName,
                lastName = user.LastName,
                userName = user.UserName,
                birthDate = user.BirthDate,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                governorate = user.Address?.Governorate ?? "",
                city = user.Address?.City ?? "",
                street = user.Address?.Street ?? "",
                addressId = user.AddressId,
                bio = user.Bio ?? "",
                profilePicture = user.ProfilePicture?.Path ?? "https://placehold.co/100x100?text=Profile",
                categoryId = user.CategoryID,
                categoryName = user.Category?.Name,
                experienceYears = user.ExperienceYears,
                price = user.Price,
                isShop = user.IsShop == true,
                shopName = user.ShopName,
                isTechnician = roles.Contains("Technician"),
                roles = roles
            });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileFormRequest model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
                return Unauthorized();

            var currentUser = await _context.Users
                .Include(u => u.Address)
                .Include(u => u.ProfilePicture)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (currentUser == null)
                return NotFound(new { message = "المستخدم غير موجود." });

            var roles = await _userManager.GetRolesAsync(currentUser);
            bool isTech = roles.Contains("Technician");

            if (isTech)
            {
                if (model.CategoryId == null)
                    return BadRequest(new { message = "يرجى اختيار فئة فني." });

                currentUser.CategoryID = model.CategoryId;

                if (currentUser.IsShop != true)
                {
                    if (model.ExperienceYears == null)
                        return BadRequest(new { message = "يرجى إدخال سنوات الخبرة." });
                    if (model.Price == null || model.Price <= 0)
                        return BadRequest(new { message = "يرجى إدخال سعر الخدمة." });

                    currentUser.ExperienceYears = model.ExperienceYears;
                    currentUser.Price = model.Price;
                }
            }

            // Update Address
            var govName = await ResolveGovernorateNameAsync(model.Governorate);
            var cityName = await ResolveCityNameAsync(model.City);

            if (currentUser.Address != null)
            {
                currentUser.Address.Governorate = string.IsNullOrWhiteSpace(govName) ? currentUser.Address.Governorate : govName;
                currentUser.Address.City = string.IsNullOrWhiteSpace(cityName) ? currentUser.Address.City : cityName;
                currentUser.Address.Street = model.Street ?? currentUser.Address.Street;
            }
            else
            {
                var address = new Address
                {
                    Governorate = govName,
                    City = cityName,
                    Street = model.Street ?? ""
                };
                await _addressRepository.Add(address);
                currentUser.AddressId = address.Id;
            }

            // Update Profile Picture (Base64)
            if (model.UploadedImage != null && model.UploadedImage.Length > 0)
            {
                using var ms = new MemoryStream();
                await model.UploadedImage.CopyToAsync(ms);
                string base64Image = Convert.ToBase64String(ms.ToArray());
                string dataUri = $"data:{model.UploadedImage.ContentType};base64,{base64Image}";

                if (currentUser.ProfilePicture != null)
                {
                    currentUser.ProfilePicture.Path = dataUri;
                }
                else
                {
                    var picture = new ProfilePicture { Path = dataUri };
                    await _pictureRepository.Add(picture);
                    currentUser.ProfilePictureId = picture.Id;
                }
            }

            currentUser.FirstName = model.FirstName;
            currentUser.LastName = model.LastName;
            currentUser.BirthDate = model.BirthDate;
            currentUser.Email = model.Email;
            currentUser.PhoneNumber = model.PhoneNumber;
            currentUser.Bio = model.Bio;
            if (currentUser.IsShop == true && !string.IsNullOrWhiteSpace(model.ShopName))
            {
                currentUser.ShopName = model.ShopName;
            }

            await _context.SaveChangesAsync();

            var updatedInfo = await BuildAuthUserResponseAsync(currentUser);
            return Ok(new
            {
                message = "تم تحديث الملف الشخصي بنجاح.",
                user = updatedInfo
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out long userId))
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    user.IsOnline = false;
                    await _userManager.UpdateAsync(user);
                    await _hubContext.Clients.All.SendAsync("ReceiveUserStatus", user.Id, false);
                }
            }
            return Ok(new { message = "تم تسجيل الخروج بنجاح." });
        }
    }

    public class GoogleAuthRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PictureUrl { get; set; }
        public string? ProviderKey { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileFormRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public int? CategoryId { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? Price { get; set; }
        public string? ShopName { get; set; }
        public IFormFile? UploadedImage { get; set; }
    }
}
