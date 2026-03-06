using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationWorker.Services
{
    /// <summary>
    /// Сервис для отправки Email уведомлений и рассылок
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

        /// <summary>
        /// Отправляет одно письмо на указанный адрес.
        /// </summary>
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

        /// <summary>
        /// Рассылка: отправляет одно и то же письмо на несколько адресов.
        /// </summary>
        /// <param name="toAddresses">Список email-адресов</param>
        /// <param name="subject">Тема</param>
        /// <param name="message">Текст (HTML)</param>
        /// <param name="delayBetweenMs">Пауза между отправками в мс (0 — без паузы)</param>
        /// <param name="cancellationToken">Отмена</param>
        /// <returns>Число успешно отправленных</returns>
        public async Task<int> SendBulkAsync(
            IReadOnlyList<string> toAddresses,
            string? subject,
            string message,
            int delayBetweenMs = 100,
            CancellationToken cancellationToken = default)
        {
            if (toAddresses == null || toAddresses.Count == 0)
                return 0;

            var sent = 0;
            for (var i = 0; i < toAddresses.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var to = toAddresses[i]?.Trim();
                if (string.IsNullOrWhiteSpace(to)) continue;
                if (!IsValidEmail(to))
                {
                    _logger.LogWarning("Некорректный email в рассылке, пропуск: {Email}", to);
                    continue;
                }
                var ok = await SendAsync(to, subject, message);
                if (ok) sent++;
                if (delayBetweenMs > 0 && i < toAddresses.Count - 1)
                    await Task.Delay(delayBetweenMs, cancellationToken);
            }
            _logger.LogInformation("Рассылка: отправлено {Sent} из {Total} писем", sent, toAddresses.Count);
            return sent;
        }

        /// <summary>
        /// Разбирает строку с несколькими адресами (запятая, точка с запятой, пробел, перенос строки).
        /// </summary>
        public static IReadOnlyList<string> ParseAddressList(string? contactInfo)
        {
            if (string.IsNullOrWhiteSpace(contactInfo)) return Array.Empty<string>();
            return contactInfo
                .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 254) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}

