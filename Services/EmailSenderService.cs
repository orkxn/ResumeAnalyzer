using System.Net;
using System.Net.Mail;

namespace ResumeAnalyzer.Services
{
    public class EmailSenderService
    {
        private readonly IConfiguration _configuration;

        public EmailSenderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var host = _configuration["SMTP_HOST"] ?? throw new InvalidOperationException("SMTP_HOST is missing in environment.");
            var portStr = _configuration["SMTP_PORT"] ?? throw new InvalidOperationException("SMTP_PORT is missing in environment.");
            var fromEmail = _configuration["SMTP_EMAIL"] ?? throw new InvalidOperationException("SMTP_EMAIL is missing in environment.");
            var password = _configuration["SMTP_PASSWORD"] ?? throw new InvalidOperationException("SMTP_PASSWORD is missing in environment.");

            if (!int.TryParse(portStr, out var port))
            {
                port = 587;
            }

            using var smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "CV Analiz"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
