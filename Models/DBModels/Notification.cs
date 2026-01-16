using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NotificationWorker.Models.DBModels;

/// <summary>
/// Таблица уведомлений для отправки через различные каналы
/// </summary>
[Table("notifications")]
public partial class Notification
{
    [Key]
    [Column("id", TypeName = "character varying")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// ID клиента, которому отправляется уведомление
    /// </summary>
    [Column("client_id", TypeName = "character varying")]
    public string? ClientId { get; set; }

    /// <summary>
    /// Канал отправки: email, telegram, whatsapp
    /// </summary>
    [Column("channel", TypeName = "character varying")]
    public string? Channel { get; set; }

    /// <summary>
    /// Контактная информация для отправки (email, телефон, telegram chat_id)
    /// </summary>
    [Column("contact_info", TypeName = "character varying")]
    public string? ContactInfo { get; set; }

    /// <summary>
    /// Тема уведомления
    /// </summary>
    [Column("subject", TypeName = "character varying")]
    public string? Subject { get; set; }

    /// <summary>
    /// Тело сообщения (HTML или текст)
    /// </summary>
    [Column("message", TypeName = "text")]
    public string? Message { get; set; }

    /// <summary>
    /// Статус уведомления:
    /// new - новое, ожидает обработки
    /// processing - в процессе обработки
    /// sent - отправлено
    /// failed - ошибка отправки
    /// </summary>
    [Column("status", TypeName = "character varying")]
    public string? Status { get; set; }

    /// <summary>
    /// Дата создания
    /// </summary>
    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Дата обработки
    /// </summary>
    [Column("processed_at", TypeName = "timestamp without time zone")]
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Дата отправки
    /// </summary>
    [Column("sent_at", TypeName = "timestamp without time zone")]
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Количество попыток отправки
    /// </summary>
    [Column("retry_count")]
    public int? RetryCount { get; set; }

    /// <summary>
    /// Сообщение об ошибке (если была)
    /// </summary>
    [Column("error_message", TypeName = "text")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Дополнительные данные в формате JSON
    /// </summary>
    [Column("metadata", TypeName = "text")]
    public string? Metadata { get; set; }

    [ForeignKey("ClientId")]
    [InverseProperty("Notifications")]
    public virtual OrganizationClient? ClientNavigation { get; set; }
}

