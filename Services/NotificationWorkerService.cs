using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationWorker.Models.DBModels;
using NotificationWorker.Helpers;

namespace NotificationWorker.Services
{
    /// <summary>
    /// Фоновый воркер, который обрабатывает таблицу notifications
    /// Выбирает записи со статусом "new", переводит на "processing" и отправляет через каналы
    /// </summary>
    public class NotificationWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationWorkerService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10); // Проверка каждые 10 секунд
        private readonly int _batchSize = 50; // Размер пачки для обработки

        public NotificationWorkerService(
            IServiceProvider serviceProvider,
            ILogger<NotificationWorkerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationWorkerService запущен. Проверка каждые {Interval} секунд", _checkInterval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNotificationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в NotificationWorkerService");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessNotificationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();
            var telegramSender = scope.ServiceProvider.GetRequiredService<TelegramSender>();
            var wappiWhatsAppSender = scope.ServiceProvider.GetRequiredService<WappiWhatsAppSender>();

            // Выбираем пачку уведомлений со статусом "new"
            var notifications = await db.Notifications
                .Where(n => n.Status == "new")
                .OrderBy(n => n.CreatedAt)
                .Take(_batchSize)
                .ToListAsync(cancellationToken);

            if (!notifications.Any())
            {
                return; // Нет новых уведомлений
            }

            _logger.LogInformation("Найдено {Count} новых уведомлений для обработки", notifications.Count);

            foreach (var notification in notifications)
            {
                try
                {
                    var channel = notification.Channel?.ToLower() ?? "email";

                    // Обновляем статус на "processing"
                    notification.Status = "processing";
                    notification.ProcessedAt = ParsersHelper.NowForTimestamp();
                    await db.SaveChangesAsync(cancellationToken);

                    // Отправляем уведомление в зависимости от канала
                    bool success = false;

                    switch (channel)
                    {
                        case "email":
                            if (!string.IsNullOrWhiteSpace(notification.ContactInfo))
                            {
                                success = await emailSender.SendAsync(
                                    notification.ContactInfo.Trim(),
                                    notification.Subject,
                                    notification.Message ?? "");
                            }
                            break;

                        case "telegram":
                            if (!string.IsNullOrWhiteSpace(notification.ContactInfo))
                            {
                                success = await telegramSender.SendAsync(
                                    notification.ContactInfo,
                                    notification.Subject,
                                    notification.Message ?? "");
                            }
                            break;

                        case "whatsapp":
                            if (!string.IsNullOrWhiteSpace(notification.ContactInfo))
                            {
                                // Используется Wappi.pro; WABA (WhatsAppSender) не используется, оставлен на потом
                                success = await wappiWhatsAppSender.SendAsync(
                                    notification.ContactInfo,
                                    notification.Subject,
                                    notification.Message ?? "");
                            }
                            break;

                        default:
                            _logger.LogWarning("Неизвестный канал уведомления: {Channel} для уведомления {NotificationId}", 
                                channel, notification.Id);
                            notification.Status = "failed";
                            notification.ErrorMessage = $"Неизвестный канал: {channel}";
                            break;
                    }

                    // Обновляем статус в зависимости от результата
                    if (channel != "email" && channel != "telegram" && channel != "whatsapp")
                    {
                        // Уже обработано выше
                    }
                    else if (success)
                    {
                        notification.Status = "sent";
                        notification.SentAt = ParsersHelper.NowForTimestamp();
                        notification.ErrorMessage = null;
                    }
                    else
                    {
                        notification.Status = "failed";
                        notification.RetryCount = (notification.RetryCount ?? 0) + 1;
                        
                        // Если превышен лимит попыток, оставляем как failed
                        if (notification.RetryCount > 5)
                        {
                            notification.ErrorMessage = "Превышен лимит попыток отправки";
                        }
                        else
                        {
                            // Возвращаем статус на "new" для повторной попытки
                            notification.Status = "new";
                            notification.ProcessedAt = null;
                        }
                    }

                    await db.SaveChangesAsync(cancellationToken);

                    if (success)
                    {
                        _logger.LogInformation(
                            "Уведомление {NotificationId} отправлено через канал {Channel}",
                            notification.Id,
                            channel);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Ошибка при обработке уведомления {NotificationId}",
                        notification.Id);

                    // Возвращаем статус обратно на "new" для повторной попытки
                    notification.Status = "new";
                    notification.ProcessedAt = null;
                    notification.RetryCount = (notification.RetryCount ?? 0) + 1;
                    notification.ErrorMessage = ex.Message;

                    if (notification.RetryCount > 5)
                    {
                        // После 5 попыток помечаем как failed
                        notification.Status = "failed";
                    }

                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("NotificationWorkerService останавливается");
            await base.StopAsync(cancellationToken);
        }
    }
}

