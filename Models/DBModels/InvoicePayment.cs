using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

/// <summary>
/// записи - за какие периоды оплачены
/// </summary>
public partial class InvoicePayment
{
    public string Id { get; set; } = null!;

    public string? Invoice { get; set; }

    public DateTime? DateTo { get; set; }

    /// <summary>
    /// сумма оплаты
    /// </summary>
    public decimal? PaymentSumm { get; set; }

    /// <summary>
    /// paid - уже оплатил
    /// non_paid - еще не оплатил
    /// anulated - в таком случае д\с возвращается обратно на баланс по инвойсу
    /// </summary>
    public string? PaymentStatus { get; set; }

    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// какой месяц или год
    /// если периодичность месяц или год
    /// </summary>
    public string? PeriodValue { get; set; }

    public virtual Invoice? InvoiceNavigation { get; set; }

    public virtual ICollection<Transaction> Transactions { get; } = new List<Transaction>();
}
