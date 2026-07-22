using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

/// <summary>
/// Таблица notifications — схема как в основном secore (WebApplication1).
/// </summary>
public partial class Notification
{
    public string Id { get; set; } = null!;

    public string? ClientId { get; set; }

    /// <summary>Канал: email, telegram, whatsapp</summary>
    public string? Channel { get; set; }

    /// <summary>email / телефон / telegram chat_id</summary>
    public string? ContactInfo { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    /// <summary>new | processing | sent | failed</summary>
    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? SentAt { get; set; }

    public int? RetryCount { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>Доп. данные JSON</summary>
    public string? Metadata { get; set; }

    public virtual OrganizationClient? Client { get; set; }
}
