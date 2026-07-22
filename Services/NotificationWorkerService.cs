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
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);
        private readonly int _batchSize = 50;

        public NotificationWorkerService(
            IServiceProvider serviceProvider,
            ILogger<NotificationWorkerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "NotificationWorkerService запущен. Проверка каждые {Interval} секунд, BatchSize={BatchSize}",
                _checkInterval.TotalSeconds,
                _batchSize);

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

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("NotificationWorkerService цикл остановлен");
        }

        private async Task ProcessNotificationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();
            var telegramSender = scope.ServiceProvider.GetRequiredService<TelegramSender>();
            var wappiWhatsAppSender = scope.ServiceProvider.GetRequiredService<WappiWhatsAppSender>();

            var notifications = await db.Notifications
                .Where(n => n.Status == "new")
                .OrderBy(n => n.CreatedAt)
                .Take(_batchSize)
                .ToListAsync(cancellationToken);

            if (!notifications.Any())
            {
                return;
            }

            _logger.LogInformation("Найдено {Count} новых уведомлений для обработки", notifications.Count);

            foreach (var notification in notifications)
            {
                try
                {
                    var channel = notification.Channel?.ToLower() ?? "email";

                    notification.Status = "processing";
                    notification.ProcessedAt = ParsersHelper.NowForTimestamp();
                    await db.SaveChangesAsync(cancellationToken);

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
                            else
                            {
                                _logger.LogWarning(
                                    "Уведомление {NotificationId}: пустой ContactInfo для email. ClientId={ClientId}",
                                    notification.Id,
                                    notification.ClientId);
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
                            else
                            {
                                _logger.LogWarning(
                                    "Уведомление {NotificationId}: пустой ContactInfo для telegram. ClientId={ClientId}",
                                    notification.Id,
                                    notification.ClientId);
                            }
                            break;

                        case "whatsapp":
                            if (!string.IsNullOrWhiteSpace(notification.ContactInfo))
                            {
                                success = await wappiWhatsAppSender.SendAsync(
                                    notification.ContactInfo,
                                    notification.Subject,
                                    notification.Message ?? "");
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Уведомление {NotificationId}: пустой ContactInfo для whatsapp. ClientId={ClientId}",
                                    notification.Id,
                                    notification.ClientId);
                            }
                            break;

                        default:
                            _logger.LogWarning(
                                "Неизвестный канал {Channel} для уведомления {NotificationId}. ClientId={ClientId}",
                                channel,
                                notification.Id,
                                notification.ClientId);
                            notification.Status = "failed";
                            notification.ErrorMessage = $"Неизвестный канал: {channel}";
                            break;
                    }

                    if (channel != "email" && channel != "telegram" && channel != "whatsapp")
                    {
                        // уже failed выше
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

                        if (notification.RetryCount > 5)
                        {
                            notification.ErrorMessage = "Превышен лимит попыток отправки";
                        }
                        else
                        {
                            notification.Status = "new";
                            notification.ProcessedAt = null;
                        }
                    }

                    await db.SaveChangesAsync(cancellationToken);

                    if (success)
                    {
                        _logger.LogInformation(
                            "Уведомление {NotificationId} отправлено. Channel={Channel}, ClientId={ClientId}",
                            notification.Id,
                            channel,
                            notification.ClientId);
                    }
                    else if (channel is "email" or "telegram" or "whatsapp")
                    {
                        _logger.LogWarning(
                            "Уведомление {NotificationId} не отправлено. Channel={Channel}, ClientId={ClientId}, Status={Status}, RetryCount={RetryCount}",
                            notification.Id,
                            channel,
                            notification.ClientId,
                            notification.Status,
                            notification.RetryCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Ошибка при обработке уведомления {NotificationId}. ClientId={ClientId}, Channel={Channel}",
                        notification.Id,
                        notification.ClientId,
                        notification.Channel);

                    notification.Status = "new";
                    notification.ProcessedAt = null;
                    notification.RetryCount = (notification.RetryCount ?? 0) + 1;
                    notification.ErrorMessage = ex.Message;

                    if (notification.RetryCount > 5)
                    {
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
