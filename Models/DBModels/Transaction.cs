using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class Transaction
{
    /// <summary>
    /// avn_txn_id
    /// </summary>
    public string Id { get; set; } = null!;

    public DateTime? TransactionDate { get; set; }

    /// <summary>
    /// success
    /// error
    /// </summary>
    public string? TransactionStatus { get; set; }

    /// <summary>
    /// сумма в тыйынах
    /// </summary>
    public decimal? Summ { get; set; }

    /// <summary>
    /// сумма транзакции в тыйынах вмесе с комиссией
    /// </summary>
    public decimal? TransactionSumm { get; set; }

    public string? Invoice { get; set; }

    /// <summary>
    /// payFromAPI - оплата через API коннектор (приход в организацию)
    /// credit - расход ранее оплаченных сумм. 
    /// (возврат денег обратно клиенту итд).
    /// payPaymentInvoice - оплата конкретного счета инвойса, внутренняя операция.
    /// </summary>
    public string? TransactionType { get; set; }

    /// <summary>
    /// txnid платежа из запроса
    /// (txnid - уникальное значение в рамках одной организации)
    /// </summary>
    public string? TxnId { get; set; }

    /// <summary>
    /// в какой системе происходила транзакция:
    /// -secore
    /// -secorePaymentSheduler
    /// </summary>
    public string? TransactionSystem { get; set; }

    /// <summary>
    /// если это оплата по API то с какого конкретно агента, id агента.
    /// </summary>
    public string? Agent { get; set; }

    /// <summary>
    /// если транзакция внутренняя и по конкретному графику, то ссылка на график
    /// </summary>
    public string? PaymentInvoice { get; set; }

    /// <summary>
    /// Нижняя комиссия от организации (тыйыны)
    /// </summary>
    public decimal? LowerCommissionFromOrg { get; set; }

    /// <summary>
    /// Верхняя комиссия от агента (тыйыны)
    /// </summary>
    public decimal? UpperCommissionFromAgent { get; set; }

    /// <summary>
    /// Нижняя комиссия к агенту (тыйыны)
    /// </summary>
    public decimal? LowerCommissionToAgent { get; set; }

    public virtual Agent? AgentNavigation { get; set; }

    public virtual Invoice? InvoiceNavigation { get; set; }

    public virtual InvoicePayment? PaymentInvoiceNavigation { get; set; }
}
