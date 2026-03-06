using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class OrganizationService
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? Organization { get; set; }

    /// <summary>
    /// если fixed_sum = 1, то тут будет значение фиксированной суммы
    /// </summary>
    public decimal? ServiceSumm { get; set; }

    /// <summary>
    /// если 1 то услуга с фиксированной суммой
    /// </summary>
    public int? FixedSum { get; set; }

    /// <summary>
    /// мин сумма в тыйынах
    /// </summary>
    public decimal? MinSumm { get; set; }

    /// <summary>
    /// макс сумма в тыйынах
    /// </summary>
    public decimal? MaxSumm { get; set; }

    public int? Isdeleted { get; set; }

    public virtual ICollection<InvoiceService> InvoiceServices { get; } = new List<InvoiceService>();

    public virtual Organization? OrganizationNavigation { get; set; }
}
