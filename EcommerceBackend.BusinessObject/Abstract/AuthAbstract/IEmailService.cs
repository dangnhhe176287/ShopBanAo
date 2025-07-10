using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace EcommerceBackend.BusinessObject.Abstract.AuthAbstract
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendOtpEmailAsync(string toEmail, string otpCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_configuration["Email:SenderName"], _configuration["Email:SenderEmail"]));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = body;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_configuration["Email:SmtpServer"], int.Parse(_configuration["Email:Port"]), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var subject = "Mã OTP đặt lại mật khẩu";
            var body = $@"
                <html>
                <body>
                    <h2>Đặt lại mật khẩu</h2>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình.</p>
                    <p>Mã OTP của bạn là: <strong>{otpCode}</strong></p>
                    <p>Mã này sẽ hết hạn sau 5 phút.</p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
