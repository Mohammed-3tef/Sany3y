using Microsoft.AspNetCore.Identity.UI.Services;
using Sany3y.Infrastructure.Models;

namespace Sany3y.API.Services
{
    public class ApiEmailService
    {
        private readonly IEmailSender _emailSender;

        public ApiEmailService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async System.Threading.Tasks.Task SendConfirmationAsync(User user, string callbackUrl)
        {
            var msg = $@"
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
                        .content {{ padding: 40px 30px; color: #333333; line-height: 1.8; text-align: right; }}
                        .welcome-text {{ font-size: 20px; font-weight: 600; color: #198754; margin-bottom: 20px; }}
                        .button-container {{ text-align: center; margin: 30px 0; }}
                        .button {{ display: inline-block; padding: 15px 40px; background-color: #ffc107; color: #000000; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 16px; }}
                        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 13px; color: #6c757d; border-top: 1px solid #eee; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1><span style='color: #ffc107;'>✦</span> Sany3y | صنايعي</h1>
                        </div>
                        <div class='content'>
                            <div class='welcome-text'>مرحباً بك في عائلة صنايعي! 👋</div>
                            <p>نحن سعداء جداً بانضمامك إلينا. يرجى الضغط على الزر أدناه لتأكيد بريدك الإلكتروني وتفعيل حسابك:</p>
                            <div class='button-container'>
                                <a href='{callbackUrl}' class='button'>تأكيد البريد الإلكتروني</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} Sany3y. جميع الحقوق محفوظة.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailSender.SendEmailAsync(user.Email, "تأكيد حسابك في صنايعي", msg);
            }
        }

        public async System.Threading.Tasks.Task SendPasswordResetAsync(string email, string resetUrl)
        {
            var msg = $@"
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
                        .content {{ padding: 40px 30px; color: #333333; line-height: 1.8; text-align: right; }}
                        .welcome-text {{ font-size: 20px; font-weight: 600; color: #198754; margin-bottom: 20px; }}
                        .button-container {{ text-align: center; margin: 30px 0; }}
                        .button {{ display: inline-block; padding: 15px 40px; background-color: #ffc107; color: #000000; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 16px; }}
                        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 13px; color: #6c757d; border-top: 1px solid #eee; }}
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
                            <div class='button-container'>
                                <a href='{resetUrl}' class='button'>تغيير كلمة المرور</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} Sany3y. جميع الحقوق محفوظة.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await _emailSender.SendEmailAsync(email, "Password Change Request", msg);
        }
    }
}
