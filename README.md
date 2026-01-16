# NotificationWorker

Фоновый сервис для обработки и отправки уведомлений через различные каналы связи.

## Описание

`NotificationWorker` - это консольное приложение (Background Service), которое:
- Постоянно мониторит таблицу `notifications` в базе данных
- Находит уведомления со статусом `"new"`
- Отправляет их через соответствующие каналы (Email, Telegram, WhatsApp)
- Обновляет статус уведомлений в базе данных

## Архитектура

### Компоненты:

1. **NotificationWorkerService** - фоновый сервис, который периодически проверяет таблицу
2. **EmailSender** - отправка уведомлений по Email через SMTP
3. **TelegramSender** - отправка уведомлений в Telegram через Bot API
4. **WhatsAppSender** - отправка уведомлений в WhatsApp через Business API
5. **AppDbContext** - контекст базы данных для работы с таблицами

## Как это работает

### 1. Запуск сервиса

При запуске `NotificationWorker`:
- Подключается к базе данных PostgreSQL
- Регистрирует все необходимые сервисы
- Запускает `NotificationWorkerService` как фоновый процесс

### 2. Цикл обработки

Каждые **10 секунд** сервис:
1. Выбирает до **50** уведомлений со статусом `"new"` (старые первыми)
2. Для каждого уведомления:
   - Обновляет статус на `"processing"`
   - Отправляет через соответствующий канал
   - Обновляет статус на `"sent"` (успех) или `"failed"` (ошибка)

### 3. Обработка по каналам

#### Email
- Использует SMTP сервер (настраивается в `appsettings.json`)
- Отправляет HTML сообщения

#### Telegram
- Использует Telegram Bot API
- `ContactInfo` должен содержать chat_id (например, `"@username"` или числовой ID)
- Удаляет HTML теги из сообщения

#### WhatsApp
- Использует WhatsApp Business API
- `ContactInfo` должен содержать номер телефона (например, `"+996555123456"`)
- Удаляет HTML теги из сообщения

### 4. Обработка ошибок

- При ошибке отправки увеличивается `RetryCount`
- Если `RetryCount <= 5`: статус возвращается на `"new"` для повторной попытки
- Если `RetryCount > 5`: статус устанавливается в `"failed"` и ошибка сохраняется в `ErrorMessage`

## Настройки

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Port=5432;Database=secore;Username=...;Password=..."
  },
  "NotificationSettings": {
    "Email": {
      "SmtpHost": "smtp.gmail.com",
      "SmtpPort": "587",
      "SmtpUser": "your-email@gmail.com",
      "SmtpPassword": "your-password",
      "FromEmail": "noreply@secore.kg"
    },
    "Telegram": {
      "BotToken": "your-telegram-bot-token"
    },
    "WhatsApp": {
      "ApiKey": "your-whatsapp-api-key",
      "ApiUrl": "https://api.whatsapp.com/v1/messages"
    }
  }
}
```

## Статусы уведомлений

- **`new`** - новое уведомление, ожидает обработки
- **`processing`** - в процессе обработки (отправки)
- **`sent`** - успешно отправлено
- **`failed`** - ошибка отправки (после 5 попыток)

## Запуск

```bash
cd NotificationWorker
dotnet run
```

## Логирование

Сервис логирует:
- Запуск и остановку сервиса
- Количество найденных уведомлений
- Результаты отправки каждого уведомления
- Ошибки при обработке

## Интеграция с другими сервисами

`NotificationWorker` работает независимо от других сервисов:
- `secore` (основное приложение) создает уведомления в БД
- `InvoiceSchedulerJob` создает уведомления в БД
- `NotificationWorker` обрабатывает и отправляет уведомления

Все сервисы работают с одной базой данных, но могут быть развернуты на разных серверах.

