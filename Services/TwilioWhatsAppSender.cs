using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NotificationWorker.Services
{
    /// <summary>
    /// Отправка WhatsApp через Twilio API.
    /// Используется для канала whatsapp вместо WABA (Meta Cloud API) из-за блокировок со стороны WhatsApp.
    /// </summary>
    public class TwilioWhatsAppSender
    {
        private readonly ILogger<TwilioWhatsAppSender> _logger;
        private readonly IConfiguration _configuration;

        public TwilioWhatsAppSender(
            ILogger<TwilioWhatsAppSender> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Отправляет текстовое сообщение в WhatsApp через Twilio.
        /// </summary>
        /// <param name="phone">Номер получателя (международный формат, с + или без)</param>
        /// <param name="subject">Тема (добавляется в начало сообщения)</param>
        /// <param name="message">Текст сообщения</param>
        public async Task<bool> SendAsync(string phone, string? subject, string message)
        {
            try
            {
                var accountSid = _configuration["NotificationSettings:Twilio:AccountSid"];
                var authToken = _configuration["NotificationSettings:Twilio:AuthToken"];
                var from = _configuration["NotificationSettings:Twilio:WhatsAppFrom"];

                if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(from))
                {
                    _logger.LogWarning("Twilio не настроен (AccountSid, AuthToken или WhatsAppFrom). WhatsApp на {Phone} не отправлен.", phone);
                    return false;
                }

                var toE164 = NormalizeToE164(phone ?? "");
                if (string.IsNullOrEmpty(toE164))
                {
                    _logger.LogWarning("Некорректный номер для WhatsApp: {Phone}", phone);
                    return false;
                }

                var fromNumber = from!.Trim();
                if (!fromNumber.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
                    fromNumber = "whatsapp:" + fromNumber.TrimStart('+');

                var toNumber = toE164.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase) ? toE164 : "whatsapp:" + toE164;

                var plainMessage = Regex.Replace(message ?? "", "<.*?>", string.Empty);
                var body = string.IsNullOrWhiteSpace(subject)
                    ? plainMessage
                    : $"{subject}\n\n{plainMessage}";

                TwilioClient.Init(accountSid, authToken);

                var resource = await MessageResource.CreateAsync(
                    from: new PhoneNumber(fromNumber),
                    to: new PhoneNumber(toNumber),
                    body: body
                );

                if (resource.Status == MessageResource.StatusEnum.Queued ||
                    resource.Status == MessageResource.StatusEnum.Sent ||
                    resource.Status == MessageResource.StatusEnum.Delivered)
                {
                    _logger.LogInformation("WhatsApp (Twilio) отправлено на {Phone}, Sid: {Sid}", phone, resource.Sid);
                    return true;
                }

                _logger.LogWarning("WhatsApp (Twilio) неожиданный статус на {Phone}: {Status}, Sid: {Sid}", phone, resource.Status, resource.Sid);
                return resource.Status != MessageResource.StatusEnum.Failed && resource.ErrorCode == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке WhatsApp (Twilio) на {Phone}", phone);
                return false;
            }
        }

        private static string NormalizeToE164(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "";
            var digits = Regex.Replace(phone, @"\D", "");
            return digits.Length > 0 ? "+" + digits : "";
        }
    }
}
