using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationWorker.Services
{
    /// <summary>
    /// Сервис для отправки Email уведомлений
    /// </summary>
    public class EmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly IConfiguration _configuration;

        public EmailSender(
            ILogger<EmailSender> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendAsync(string to, string? subject, string message)
        {
            try
            {
                var smtpHost = _configuration["NotificationSettings:Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["NotificationSettings:Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["NotificationSettings:Email:SmtpUser"];
                var smtpPassword = _configuration["NotificationSettings:Email:SmtpPassword"];
                var fromEmail = _configuration["NotificationSettings:Email:FromEmail"] ?? smtpUser;

                if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPassword))
                {
                    _logger.LogWarning("SMTP настройки не настроены. Email не отправлен на {Email}", to);
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPassword)
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? "noreply@secore.kg"),
                    Subject = subject ?? "Уведомление",
                    Body = message,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Email уведомление отправлено на {Email}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке email на {Email}", to);
                return false;
            }
        }
    }
}

