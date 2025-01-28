using System.Net;
using System.Net.Mail;
using ColdlineAPI.Application.Interfaces;
using Microsoft.Extensions.Options;
using ColdlineAPI.Infrastructure.Configurations;

namespace ColdlineAPI.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpConfig _smtpConfig;

        public EmailService(IOptions<SmtpConfig> smtpConfig)
        {
            _smtpConfig = smtpConfig.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var smtpClient = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port)
            {
                Credentials = new NetworkCredential(_smtpConfig.User, _smtpConfig.Password),
                EnableSsl = _smtpConfig.UseSSL
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpConfig.User),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
