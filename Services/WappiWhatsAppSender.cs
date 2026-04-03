using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationWorker.Services
{
    /// <summary>
    /// Отправка WhatsApp через API сервиса Wappi.pro (HTTP, профиль WhatsApp Web).
    /// Документация: https://wappi.pro/api-documentation и коллекция Postman.
    /// </summary>
    public class WappiWhatsAppSender
    {
        private const string DefaultBaseUrl = "https://wappi.pro";
        private const string DefaultSendPath = "/api/async/message/send";

        private readonly ILogger<WappiWhatsAppSender> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public WappiWhatsAppSender(
            ILogger<WappiWhatsAppSender> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Отправляет текстовое сообщение через Wappi (асинхронный метод API).
        /// Возвращает true при HTTP 2xx и если ответ не содержит явной ошибки.
        /// </summary>
        public async Task<bool> SendAsync(string phone, string? subject, string message)
        {
            try
            {
                var apiToken = _configuration["NotificationSettings:Wappi:ApiToken"];
                var profileId = _configuration["NotificationSettings:Wappi:ProfileId"];
                var baseUrl = _configuration["NotificationSettings:Wappi:BaseUrl"] ?? DefaultBaseUrl;
                var sendPath = _configuration["NotificationSettings:Wappi:SendMessagePath"] ?? DefaultSendPath;
                var timeoutSec = _configuration.GetValue<int?>("NotificationSettings:Wappi:RequestTimeoutSec") ?? 25;

                if (string.IsNullOrWhiteSpace(apiToken) || string.IsNullOrWhiteSpace(profileId))
                {
                    _logger.LogWarning("Wappi не настроен (ApiToken или ProfileId). WhatsApp на {Phone} не отправлен.", phone);
                    return false;
                }

                var recipient = NormalizeRecipient(phone ?? "");
                if (string.IsNullOrEmpty(recipient))
                {
                    _logger.LogWarning("Некорректный номер для WhatsApp (Wappi): {Phone}", phone);
                    return false;
                }

                var plainMessage = Regex.Replace(message ?? "", "<.*?>", string.Empty);
                var body = string.IsNullOrWhiteSpace(subject)
                    ? plainMessage
                    : $"{subject}\n\n{plainMessage}";

                var url = $"{baseUrl.TrimEnd('/')}{sendPath}?profile_id={Uri.EscapeDataString(profileId)}";

                // Доп. параметры из примера асинхронной отправки
                var timeoutFrom = _configuration["NotificationSettings:Wappi:TimeoutFrom"];
                var timeoutTo = _configuration["NotificationSettings:Wappi:TimeoutTo"];
                if (!string.IsNullOrWhiteSpace(timeoutFrom))
                    url += $"&timeout_from={Uri.EscapeDataString(timeoutFrom)}";
                if (!string.IsNullOrWhiteSpace(timeoutTo))
                    url += $"&timeout_to={Uri.EscapeDataString(timeoutTo)}";

                var payload = new Dictionary<string, string>
                {
                    ["recipient"] = recipient,
                    ["body"] = body
                };

                using var httpClient = _httpClientFactory.CreateClient();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(5, timeoutSec)));
                string responseContent;
                HttpResponseMessage response;

                // 1) JSON (явный формат)
                using (var jsonRequest = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    jsonRequest.Headers.TryAddWithoutValidation("Authorization", apiToken);
                    jsonRequest.Content = JsonContent.Create(payload);
                    response = await httpClient.SendAsync(jsonRequest, cts.Token);
                    responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                }

                // 2) Fallback: form-url-encoded (как часто ожидают endpoint'ы с curl --data)
                if (!response.IsSuccessStatusCode)
                {
                    response.Dispose();
                    using var formRequest = new HttpRequestMessage(HttpMethod.Post, url);
                    formRequest.Headers.TryAddWithoutValidation("Authorization", apiToken);
                    formRequest.Content = new FormUrlEncodedContent(payload);
                    response = await httpClient.SendAsync(formRequest, cts.Token);
                    responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Wappi API ошибка при отправке на {Phone}. Status: {Status}, Response: {Response}",
                        phone, response.StatusCode, responseContent);
                    response.Dispose();
                    return false;
                }

                if (LooksLikeApiError(responseContent))
                {
                    _logger.LogWarning("Wappi API вернул ошибку в теле ответа на {Phone}: {Response}", phone, responseContent);
                    return false;
                }

                _logger.LogInformation("WhatsApp (Wappi) отправлено на {Phone}", phone);
                response.Dispose();
                return true;
            }
            catch (OperationCanceledException oce)
            {
                _logger.LogError(oce, "Wappi API timeout при отправке на {Phone}. Проверьте доступность endpoint и формат тела запроса.", phone);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке WhatsApp (Wappi) на {Phone}", phone);
                return false;
            }
        }

        /// <summary>
        /// Номер только цифрами (международный формат без +), как ожидает Wappi для recipient.
        /// </summary>
        private static string NormalizeRecipient(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "";
            var digits = Regex.Replace(phone, @"\D", "");
            return digits.Length > 0 ? digits : "";
        }

        private static bool LooksLikeApiError(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("status", out var st) && st.ValueKind == JsonValueKind.String)
                {
                    var s = st.GetString()?.ToLowerInvariant();
                    if (s is "error" or "failed") return true;
                }
                if (root.TryGetProperty("error", out _) || root.TryGetProperty("errors", out _))
                    return true;
            }
            catch
            {
                // не JSON — считаем успехом, если HTTP уже 2xx
            }
            return false;
        }
    }
}
