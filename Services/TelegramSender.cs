using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationWorker.Services
{
    /// <summary>
    /// Сервис для отправки Telegram уведомлений
    /// </summary>
    public class TelegramSender
    {
        private readonly ILogger<TelegramSender> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public TelegramSender(
            ILogger<TelegramSender> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> SendAsync(string chatId, string? subject, string message)
        {
            try
            {
                var botToken = _configuration["NotificationSettings:Telegram:BotToken"];

                if (string.IsNullOrWhiteSpace(botToken))
                {
                    _logger.LogWarning("Telegram Bot Token не настроен. Уведомление не отправлено на {ChatId}", chatId);
                    return false;
                }

                // Убираем HTML теги из сообщения для Telegram
                var plainMessage = System.Text.RegularExpressions.Regex.Replace(message ?? "", "<.*?>", string.Empty);
                var fullMessage = $"*{subject ?? "Уведомление"}*\n\n{plainMessage}";
                var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

                var payload = new
                {
                    chat_id = chatId,
                    text = fullMessage,
                    parse_mode = "Markdown"
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Telegram уведомление отправлено на {ChatId}", chatId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Ошибка отправки Telegram на {ChatId}: {Response}", chatId, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке Telegram на {ChatId}", chatId);
                return false;
            }
        }
    }
}

