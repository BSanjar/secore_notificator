using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class InvoiceService
{
    public string Id { get; set; } = null!;

    public string? Invoice { get; set; }

    public string? Service { get; set; }

    /// <summary>
    /// стоимость сервиса
    /// </summary>
    public decimal? ServiceSumm { get; set; }

    public virtual Invoice? InvoiceNavigation { get; set; }

    public virtual OrganizationService? ServiceNavigation { get; set; }
}
