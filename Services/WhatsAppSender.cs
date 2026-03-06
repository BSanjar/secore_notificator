using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationWorker.Services
{
    /// <summary>
    /// Сервис отправки уведомлений через WhatsApp Cloud API (Meta).
    /// Документация: https://developers.facebook.com/docs/whatsapp/cloud-api
    /// </summary>
    public class WhatsAppSender
    {
        private const string DefaultBaseUrl = "https://graph.facebook.com";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

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

        /// <summary>
        /// Отправляет текстовое сообщение через WhatsApp Cloud API.
        /// </summary>
        /// <param name="phone">Номер получателя (международный формат, с + или без)</param>
        /// <param name="subject">Тема (добавляется в начало сообщения)</param>
        /// <param name="message">Текст сообщения</param>
        /// <returns>true при успешной отправке</returns>
        public async Task<bool> SendAsync(string phone, string? subject, string message)
        {
            try
            {
                var accessToken = _configuration["NotificationSettings:WhatsApp:AccessToken"];
                var phoneNumberId = _configuration["NotificationSettings:WhatsApp:PhoneNumberId"];
                var wabaId = _configuration["NotificationSettings:WhatsApp:WhatsAppBusinessAccountId"];
                var apiVersion = _configuration["NotificationSettings:WhatsApp:ApiVersion"] ?? "v21.0";
                var baseUrl = _configuration["NotificationSettings:WhatsApp:BaseUrl"] ?? DefaultBaseUrl;

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    _logger.LogWarning("WhatsApp Cloud API не настроен (AccessToken). Уведомление на {Phone} не отправлено.", phone);
                    return false;
                }

                // Если задан только ID аккаунта (WABA), получаем Phone Number ID через API
                if (string.IsNullOrWhiteSpace(phoneNumberId) && !string.IsNullOrWhiteSpace(wabaId))
                {
                    phoneNumberId = await ResolvePhoneNumberIdAsync(baseUrl, apiVersion, wabaId, accessToken);
                    if (string.IsNullOrWhiteSpace(phoneNumberId))
                    {
                        _logger.LogWarning("Не удалось получить Phone Number ID по WhatsApp Business Account ID. Уведомление на {Phone} не отправлено.", phone);
                        return false;
                    }
                }

                if (string.IsNullOrWhiteSpace(phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp: укажите PhoneNumberId или WhatsAppBusinessAccountId в настройках. Уведомление на {Phone} не отправлено.", phone);
                    return false;
                }

                var to = NormalizePhoneNumber(phone ?? "");
                if (string.IsNullOrEmpty(to))
                {
                    _logger.LogWarning("Некорректный номер телефона для WhatsApp: {Phone}", phone);
                    return false;
                }

                var plainMessage = Regex.Replace(message ?? "", "<.*?>", string.Empty);
                var subjectPart = subject ?? "Уведомление";
                var body = string.IsNullOrWhiteSpace(subject)
                    ? plainMessage
                    : $"{subjectPart}\n\n{plainMessage}";

                if (body.Length > 4096)
                {
                    _logger.LogWarning("Сообщение для WhatsApp превышает 4096 символов, будет обрезано. Номер: {Phone}", phone);
                    body = body[..4096];
                }

                var version = apiVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? apiVersion : "v" + apiVersion;
                var url = $"{baseUrl.TrimEnd('/')}/{version}/{phoneNumberId}/messages";

                // Если задан шаблон — отправляем шаблон (доставляется в любое время, в т.ч. при первом контакте).
                // Иначе — обычное текстовое сообщение (доставляется только если получатель писал вам в последние 24 часа).
                var templateName = _configuration["NotificationSettings:WhatsApp:NotificationTemplateName"];
                var templateLanguage = _configuration["NotificationSettings:WhatsApp:NotificationTemplateLanguage"] ?? "ru";
                var templateHasBodyParams = _configuration.GetValue<bool>("NotificationSettings:WhatsApp:NotificationTemplateBodyParameters");

                if (!string.IsNullOrWhiteSpace(templateName))
                {
                    IReadOnlyList<string>? bodyParams = templateHasBodyParams
                        ? new List<string> { subjectPart, plainMessage }
                        : null;
                    return await SendTemplateAsync(phone ?? "", templateName, templateLanguage, bodyParams);
                }

                var payload = new WhatsAppCloudApiRequest
                {
                    MessagingProduct = "whatsapp",
                    RecipientType = "individual",
                    To = to,
                    Type = "text",
                    Text = new WhatsAppTextBody { Body = body }
                };

                var json = JsonSerializer.Serialize(payload, JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "WhatsApp сообщение отправлено на {Phone}. Если получатель не получил — он не писал вам в последние 24 часа; используйте шаблон (NotificationTemplateName) или пусть напишет первым.",
                        phone);
                    return true;
                }

                _logger.LogWarning("Ошибка WhatsApp Cloud API при отправке на {Phone}. Status: {Status}, Response: {Response}",
                    phone, response.StatusCode, responseContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке WhatsApp на {Phone}", phone);
                return false;
            }
        }

        /// <summary>
        /// Отправляет шаблонное сообщение (доставляется в любое время, в т.ч. при первом контакте).
        /// Шаблон должен быть создан и одобрен в Meta Business Manager.
        /// </summary>
        /// <param name="phone">Номер получателя (международный формат)</param>
        /// <param name="templateName">Имя шаблона (напр. hello_world)</param>
        /// <param name="languageCode">Код языка (напр. en_US, ru)</param>
        /// <param name="bodyParameters">Параметры для подстановки в body шаблона (по порядку {{1}}, {{2}}, ...)</param>
        public async Task<bool> SendTemplateAsync(string phone, string templateName, string languageCode = "en_US", IReadOnlyList<string>? bodyParameters = null)
        {
            try
            {
                var accessToken = _configuration["NotificationSettings:WhatsApp:AccessToken"];
                var phoneNumberId = _configuration["NotificationSettings:WhatsApp:PhoneNumberId"];
                var wabaId = _configuration["NotificationSettings:WhatsApp:WhatsAppBusinessAccountId"];
                var apiVersion = _configuration["NotificationSettings:WhatsApp:ApiVersion"] ?? "v21.0";
                var baseUrl = _configuration["NotificationSettings:WhatsApp:BaseUrl"] ?? DefaultBaseUrl;

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    _logger.LogWarning("WhatsApp Cloud API не настроен (AccessToken). Шаблон на {Phone} не отправлен.", phone);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(phoneNumberId) && !string.IsNullOrWhiteSpace(wabaId))
                {
                    phoneNumberId = await ResolvePhoneNumberIdAsync(baseUrl, apiVersion, wabaId, accessToken);
                    if (string.IsNullOrWhiteSpace(phoneNumberId))
                    {
                        _logger.LogWarning("Не удалось получить Phone Number ID. Шаблон на {Phone} не отправлен.", phone);
                        return false;
                    }
                }

                if (string.IsNullOrWhiteSpace(phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp: укажите PhoneNumberId или WhatsAppBusinessAccountId. Шаблон на {Phone} не отправлен.", phone);
                    return false;
                }

                var to = NormalizePhoneNumber(phone ?? "");
                if (string.IsNullOrEmpty(to))
                {
                    _logger.LogWarning("Некорректный номер для WhatsApp: {Phone}", phone);
                    return false;
                }

                var version = apiVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? apiVersion : "v" + apiVersion;
                var url = $"{baseUrl.TrimEnd('/')}/{version}/{phoneNumberId}/messages";

                object templatePayload;
                if (bodyParameters != null && bodyParameters.Count > 0)
                {
                    templatePayload = new
                    {
                        name = templateName,
                        language = new { code = languageCode },
                        components = new[]
                        {
                            new
                            {
                                type = "body",
                                parameters = bodyParameters.Select(p => new { type = "text", text = p }).ToArray()
                            }
                        }
                    };
                }
                else
                {
                    templatePayload = new
                    {
                        name = templateName,
                        language = new { code = languageCode }
                    };
                }

                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = to,
                    type = "template",
                    template = templatePayload
                };

                var json = JsonSerializer.Serialize(payload, JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("WhatsApp шаблон {Template} отправлен на {Phone}", templateName, phone);
                    return true;
                }

                _logger.LogWarning("Ошибка WhatsApp при отправке шаблона на {Phone}. Status: {Status}, Response: {Response}",
                    phone, response.StatusCode, responseContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке WhatsApp шаблона на {Phone}", phone);
                return false;
            }
        }

        /// <summary>
        /// Получает первый Phone Number ID по ID аккаунта WhatsApp Business (WABA).
        /// GET /v21.0/{waba_id}/phone_numbers
        /// </summary>
        private async Task<string?> ResolvePhoneNumberIdAsync(string baseUrl, string apiVersion, string wabaId, string accessToken)
        {
            try
            {
                var version = apiVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? apiVersion : "v" + apiVersion;
                var url = $"{baseUrl.TrimEnd('/')}/{version}/{wabaId}/phone_numbers";
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Не удалось получить номера WhatsApp по WABA ID. Status: {Status}, Response: {Response}", response.StatusCode, json);
                    return null;
                }
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var data) && data.GetArrayLength() > 0)
                {
                    var first = data[0];
                    if (first.TryGetProperty("id", out var idEl))
                        return idEl.GetString();
                }
                _logger.LogWarning("В ответе WABA phone_numbers нет ни одного номера. Response: {Response}", json);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении Phone Number ID по WABA ID");
                return null;
            }
        }

        /// <summary>
        /// Нормализует номер: только цифры (международный формат без +).
        /// </summary>
        private static string NormalizePhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "";
            var digits = Regex.Replace(phone, @"\D", "");
            return digits.Length > 0 ? digits : "";
        }

        private class WhatsAppCloudApiRequest
        {
            [JsonPropertyName("messaging_product")]
            public string MessagingProduct { get; set; } = "";

            [JsonPropertyName("recipient_type")]
            public string RecipientType { get; set; } = "";

            [JsonPropertyName("to")]
            public string To { get; set; } = "";

            [JsonPropertyName("type")]
            public string Type { get; set; } = "";

            [JsonPropertyName("text")]
            public WhatsAppTextBody? Text { get; set; }
        }

        private class WhatsAppTextBody
        {
            [JsonPropertyName("body")]
            public string Body { get; set; } = "";
        }
    }
}
