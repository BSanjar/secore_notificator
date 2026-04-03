using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class Notification
{
    public string Id { get; set; } = null!;

    public string? ClientId { get; set; }

    public string? Channel { get; set; }

    public string? ContactInfo { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? SentAt { get; set; }

    public int? RetryCount { get; set; }

    public string? ErrorMessage { get; set; }

    public string? Metadata { get; set; }

    public string? NotificationType { get; set; }

    public string? InvoiceId { get; set; }

    public string? InvoicePaymentId { get; set; }

    public string? OrganizationId { get; set; }

    public virtual OrganizationClient? Client { get; set; }
}
