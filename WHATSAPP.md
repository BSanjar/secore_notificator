# WhatsApp: Wappi и WABA (Meta)

## Текущая схема

- **Сейчас используется:** отправка WhatsApp через **Wappi.pro** (`WappiWhatsAppSender`). Канал `whatsapp` в уведомлениях идёт через Wappi.
- **WABA (Meta Cloud API)** реализован в `WhatsAppSender` и конфиге `NotificationSettings:WhatsApp`, но **не используется** — отложен из-за блокировок со стороны WhatsApp. К нему можно вернуться позже, переключив воркер на `WhatsAppSender` вместо `WappiWhatsAppSender`.

Подробнее по Wappi: **[WAPPI.md](WAPPI.md)**.

---

## WABA (Meta Cloud API) — отложен, не используется

Ниже описана настройка для будущего использования через Meta.

### Истёк токен (Session has expired / Unauthorized)

Если в логах ошибка **"Error validating access token: Session has expired"** или **401 Unauthorized** — токен в `NotificationSettings:WhatsApp:AccessToken` истёк.

**Как получить новый токен:**

1. Откройте [Meta for Developers](https://developers.facebook.com/) → ваше приложение.
2. В меню слева: **WhatsApp** → **API Setup** (или **Getting started**).
3. В блоке **Temporary access token** нажмите **Generate** — появится новый токен (действует около 24 часов).
4. Скопируйте токен и вставьте в `appsettings.json` в поле **AccessToken** в секции `NotificationSettings:WhatsApp`.

**Для продакшена (долгоживущий токен):** используйте **System User** в Business Manager и сгенерируйте постоянный токен с правами `whatsapp_business_management` и `whatsapp_business_messaging`, либо настройте продление через Meta Business Suite. Токен из кнопки "Generate" на странице API Setup — временный (≈24 ч).

---

## Правило 24 часов

WhatsApp разрешает отправлять **произвольный текст** только тем, кто **сам написал** вашему бизнес-номеру в **последние 24 часа**. Запрос к API при этом может быть успешным (200), но сообщение **не доставляется** получателю, если он не в этом окне.

**Итог:** если получатель (996550233206) ни разу не писал на ваш номер WhatsApp Business — обычное сообщение до него не дойдёт.

---

## Что делать

### Вариант 1: Быстрый тест (без шаблона)

1. Пусть получатель **напишет первым** вашему бизнес-номеру в WhatsApp (например: «Привет»).
2. В течение **24 часов** запустите отправку снова — обычное тестовое сообщение должно прийти.

### Вариант 2: Всегда доставлять (шаблон)

Чтобы сообщения доходили **в любое время** (в том числе при первом контакте), нужно отправлять **шаблон**, одобренный Meta.

1. **Создайте шаблон в Meta**  
   [Meta for Developers](https://developers.facebook.com/) → ваше приложение → WhatsApp → Message templates → Create template.

   Пример для уведомлений:
   - **Name:** `secore_notification` (латиница, подчёркивания)
   - **Language:** Russian
   - **Category:** Utility
   - **Body:**  
     `Уведомление: {{1}}\n\n{{2}}`  
     ({{1}} — тема, {{2}} — текст; при создании укажите примеры для {{1}} и {{2}})

2. Дождитесь **одобрения** шаблона (обычно до 24 часов).

3. **Настройте приложение** в `appsettings.json`:

   ```json
   "WhatsApp": {
     "AccessToken": "...",
     "PhoneNumberId": "...",
     "ApiVersion": "v21.0",
     "NotificationTemplateName": "secore_notification",
     "NotificationTemplateLanguage": "ru"
   }
   ```

После этого все уведомления через `WhatsAppSender` будут отправляться этим шаблоном (тема и текст подставляются в {{1}} и {{2}}) и будут доставляться независимо от 24-часового окна.

---

## Справка

- [Отправка сообщений (Cloud API)](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/send-messages/)
- [Шаблоны сообщений](https://developers.facebook.com/docs/whatsapp/message-templates/guidelines/)
