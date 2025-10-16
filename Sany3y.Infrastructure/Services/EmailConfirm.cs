using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Sany3y.Infrastructure.Services
{
    public class EmailConfirm : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailConfirm(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var fromEmail = emailSettings["FromEmail"];
            var fromName = emailSettings["FromName"];
            var fromPassword = emailSettings["AppPassword"];
            var smtpServer = emailSettings["SmtpServer"];
            var port = int.Parse(emailSettings["Port"]);

            var message = new MailMessage()
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = $"<html><body>{htmlMessage}</body></html>",
                IsBodyHtml = true
            };
            message.To.Add(email);

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };

            return smtpClient.SendMailAsync(message);
        }
    }
}
