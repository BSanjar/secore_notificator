using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class OrganizationSubscription
{
    public string Id { get; set; } = null!;

    public string OrganizationId { get; set; } = null!;

    public decimal PriceTyiyn { get; set; }

    public string PeriodType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;
}
