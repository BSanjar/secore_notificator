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
            builder.Services.AddScoped<WhatsAppSender>();

            // Регистрация фонового сервиса
            builder.Services.AddHostedService<NotificationWorkerService>();

            var host = builder.Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("NotificationWorker запущен");

            await host.RunAsync();
        }
    }
}
