# Интеграция WhatsApp через Wappi.pro

Канал `whatsapp` в уведомлениях отправляется через **Wappi.pro** (`WappiWhatsAppSender`).

## Что нужно от вас

1. **Аккаунт на [wappi.pro](https://wappi.pro/)** — зарегистрироваться, оплатить тариф при необходимости (есть пробный период).

2. **Профиль WhatsApp** в личном кабинете — подключить номер по инструкции Wappi (WhatsApp Web).

3. **Данные из дашборда** (страница профиля):
   - **API-токен** (авторизация) — в заголовок `Authorization` каждого запроса.
   - **Profile ID** — идентификатор профиля, передаётся query-параметром `profile_id` (см. [документацию API](https://wappi.pro/api-documentation)).

4. **Проверить путь к методу отправки** в [коллекции Postman](https://documenter.getpostman.com/view/10932655/2s8YsnYcU3) (раздел асинхронной отправки текста).  
   По умолчанию в конфиге задано:
 - `BaseUrl`: `https://wappi.pro`
 - `SendMessagePath`: `/api/async/message/send`  
   Если у вас другой путь — укажите его в `appsettings.json` в `NotificationSettings:Wappi:SendMessagePath`.

## Настройка в appsettings.json

```json
"Wappi": {
  "ApiToken": "ваш_токен_из_кабинета",
  "ProfileId": "uuid-профиля-whatsapp",
  "BaseUrl": "https://wappi.pro",
  "SendMessagePath": "/api/async/message/send"
}
```

Тело запроса: JSON `{"recipient":"79001234567","body":"текст"}` (номер — только цифры, международный формат без `+`). Если в вашей версии API другие имена полей — сообщите поддержке Wappi или скорректируйте код в `WappiWhatsAppSender`.

## Тест при старте

При `TestOnStartup:Enabled` и заполненном `WhatsAppPhone` при запуске приложения уходит одно тестовое сообщение через Wappi.

## WABA (Meta)

Реализация через Meta Cloud API остаётся в `WhatsAppSender` и не вызывается; при необходимости можно снова переключить воркер на неё вместо Wappi.
