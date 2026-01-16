using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationWorker.Services
{
    /// <summary>
    /// Сервис для отправки WhatsApp уведомлений
    /// </summary>
    public class WhatsAppSender
    {
        private readonly ILogger<WhatsAppSender> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public WhatsAppSender(
            ILogger<WhatsAppSender> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> SendAsync(string phone, string? subject, string message)
        {
            try
            {
                var apiKey = _configuration["NotificationSettings:WhatsApp:ApiKey"];
                var apiUrl = _configuration["NotificationSettings:WhatsApp:ApiUrl"];

                if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiUrl))
                {
                    _logger.LogWarning("WhatsApp API настройки не настроены. Уведомление не отправлено на {Phone}", phone);
                    return false;
                }

                // Убираем HTML теги из сообщения для WhatsApp
                var plainMessage = System.Text.RegularExpressions.Regex.Replace(message ?? "", "<.*?>", string.Empty);
                var fullMessage = $"{subject ?? "Уведомление"}\n\n{plainMessage}";

                // Пример реализации для WhatsApp Business API
                // Адаптируйте под ваш конкретный API
                var payload = new
                {
                    to = phone,
                    message = fullMessage
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                
                var response = await httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("WhatsApp уведомление отправлено на {Phone}", phone);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Ошибка отправки WhatsApp на {Phone}: {Response}", phone, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке WhatsApp на {Phone}", phone);
                return false;
            }
        }
    }
}

