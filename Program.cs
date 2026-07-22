using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationWorker.Logging;
using NotificationWorker.Models.DBModels;
using NotificationWorker.Services;
using Serilog;

namespace NotificationWorker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            builder.Services.Configure<SecoreLoggingOptions>(
                builder.Configuration.GetSection(SecoreLoggingOptions.SectionName));

            builder.Services.AddSerilog((_, loggerConfiguration) =>
            {
                SecoreLoggingBootstrap.Configure(
                    loggerConfiguration,
                    builder.Configuration,
                    builder.Environment);
            });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionString 'DefaultConnection' не найден. Задайте его в appsettings.json или через env ConnectionStrings__DefaultConnection.");
            }

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddHttpClient();

            builder.Services.AddScoped<EmailSender>();
            builder.Services.AddScoped<TelegramSender>();
            builder.Services.AddScoped<WhatsAppSender>();
            builder.Services.AddScoped<WappiWhatsAppSender>();

            var disableWorker = builder.Configuration.GetValue<bool>("NotificationSettings:TestOnStartup:DisableWorker");
            if (!disableWorker)
            {
                builder.Services.AddHostedService<NotificationWorkerService>();
            }

            try
            {
                var host = builder.Build();

                var logRoot = SecoreLoggingBootstrap.ResolveRootPath(host.Services.GetRequiredService<IConfiguration>(), host.Services.GetRequiredService<IHostEnvironment>());
                Log.Information(
                    "SECORE notificator starting. Env={Environment} LogRoot={LogRoot} WorkerDisabled={WorkerDisabled}",
                    builder.Environment.EnvironmentName,
                    logRoot,
                    disableWorker);

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
                            Log.Information("Тестовое WhatsApp (Wappi) отправлено на {Phone}. Проверьте телефон.", testPhone);
                        else
                            Log.Warning("Тестовое WhatsApp (Wappi) не удалось отправить на {Phone}. Проверьте NotificationSettings:Wappi (ApiToken, ProfileId, BaseUrl/SendMessagePath при необходимости).", testPhone);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Ошибка при тестовой отправке WhatsApp (Wappi) на {Phone}", testPhone);
                    }
                }

                if (!disableWorker)
                {
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
                                Log.Information("Тестовая рассылка на почту: отправлено {Count} писем на {To}", sent, emailTestTo);
                            else
                                Log.Warning("Тестовая рассылка на почту не удалась. Проверьте SMTP в NotificationSettings:Email (SmtpUser, SmtpPassword, FromEmail).");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Ошибка при тестовой рассылке на почту на {To}", emailTestTo);
                        }
                    }
                }

                await host.RunAsync();
            }
            finally
            {
                Log.Information("SECORE notificator stopping.");
                Log.CloseAndFlush();
            }
        }
    }
}
