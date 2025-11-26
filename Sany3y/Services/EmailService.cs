using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Sany3y.Infrastructure.Models;
using System.Threading.Tasks;

namespace Sany3y.Services
{
    public class EmailService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        public EmailService(UserManager<User> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Send email confirmation with prebuilt callback URL.
        /// </summary>
        public async System.Threading.Tasks.Task SendConfirmationAsync(User user, string callbackUrl)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var msg = $@"
                <h2>Welcome!</h2>
                <p>Click below to confirm your email:</p>
                <a href='{callbackUrl}'>Confirm Email</a>
            ";

            await _emailSender.SendEmailAsync(user.Email, "Confirm your account", msg);
        }
    }
}
