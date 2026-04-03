using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NotificationWorker.Models.DBModels;
using NotificationWorker.Services;

namespace NotificationWorker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Настройка конфигурации
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // Настройка логирования
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Регистрация базы данных
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionString 'DefaultConnection' не найден в appsettings.json");
            }

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Регистрация HTTP клиента для Telegram и WhatsApp
            builder.Services.AddHttpClient();

            // Регистрация сервисов отправки
            builder.Services.AddScoped<EmailSender>();
            builder.Services.AddScoped<TelegramSender>();
            builder.Services.AddScoped<WhatsAppSender>();      // WABA (Meta) — не используется, оставлен на потом
            builder.Services.AddScoped<WappiWhatsAppSender>(); // WhatsApp через Wappi.pro (используется)

            // Регистрация фонового сервиса (в режиме теста можно отключать, чтобы отправлялось только сообщение при старте)
            var disableWorker = builder.Configuration.GetValue<bool>("NotificationSettings:TestOnStartup:DisableWorker");
            if (!disableWorker)
            {
                builder.Services.AddHostedService<NotificationWorkerService>();
            }

            var host = builder.Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("NotificationWorker запущен");

            // Опциональная тестовая отправка WhatsApp при старте (через Wappi; WABA не используется)
            var testEnabled = builder.Configuration.GetValue<bool>("NotificationSettings:TestOnStartup:Enabled");
            var testPhone = builder.Configuration["NotificationSettings:TestOnStartup:WhatsAppPhone"];
            if (testEnabled && !string.IsNullOrWhiteSpace(testPhone))
            {
                try
                {
                    using var scope = host.Services.CreateScope();
                    var wappiWhatsAppSender = scope.ServiceProvider.GetRequiredService<WappiWhatsAppSender>();
                    var ok = await wappiWhatsAppSender.SendAsync(testPhone, null, "тестовое сообщение асинхронно");
                    if (ok)
                        logger.LogInformation("Тестовое WhatsApp (Wappi) отправлено на {Phone}. Проверьте телефон.", testPhone);
                    else
                        logger.LogWarning("Тестовое WhatsApp (Wappi) не удалось отправить на {Phone}. Проверьте NotificationSettings:Wappi (ApiToken, ProfileId, BaseUrl/SendMessagePath при необходимости).", testPhone);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при тестовой отправке WhatsApp (Wappi) на {Phone}", testPhone);
                }
            }

            if (!disableWorker)
            {
                // Опциональная тестовая рассылка на почту при старте (NotificationSettings:TestOnStartup:Email:Enabled = true и To задан)
                var emailTestEnabled = builder.Configuration.GetValue<bool>("NotificationSettings:TestOnStartup:Email:Enabled");
                var emailTestTo = builder.Configuration["NotificationSettings:TestOnStartup:Email:To"];
                if (emailTestEnabled && !string.IsNullOrWhiteSpace(emailTestTo))
                {
                    try
                    {
                        using var scope = host.Services.CreateScope();
                        var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();
                        var addresses = EmailSender.ParseAddressList(emailTestTo);
                        int sent;
                        if (addresses.Count <= 1)
                        {
                            var ok = await emailSender.SendAsync(addresses.FirstOrDefault() ?? emailTestTo.Trim(), "Тест SECORE — рассылка", "<p>Это тестовое письмо от NotificationWorker. Если вы получили его — рассылка на почту настроена.</p>");
                            sent = ok ? 1 : 0;
                        }
                        else
                            sent = await emailSender.SendBulkAsync(addresses, "Тест SECORE — рассылка", "<p>Это тестовое письмо от NotificationWorker. Если вы получили его — рассылка на почту настроена.</p>", 100);
                        if (sent > 0)
                            logger.LogInformation("Тестовая рассылка на почту: отправлено {Count} писем на {To}", sent, emailTestTo);
                        else
                            logger.LogWarning("Тестовая рассылка на почту не удалась. Проверьте SMTP в NotificationSettings:Email (SmtpUser, SmtpPassword, FromEmail).");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Ошибка при тестовой рассылке на почту на {To}", emailTestTo);
                    }
                }
            }

            await host.RunAsync();
        }
    }
}
