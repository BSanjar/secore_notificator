using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class OrganizationSubscriptionPayment
{
    public string Id { get; set; } = null!;

    public string OrganizationId { get; set; } = null!;

    public DateTime PaidAt { get; set; }

    public decimal AmountTyiyn { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;
}
