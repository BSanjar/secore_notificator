using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

/// <summary>
/// Настройки организации (1:1 с organization)
/// </summary>
public partial class OrganizationSetting
{
    /// <summary>
    /// PK и FK на organization.id
    /// </summary>
    public string OrganizationId { get; set; } = null!;

    /// <summary>
    /// отключить выбор услуги при создании счёта; ввод названия и цены вручную
    /// </summary>
    public bool DisableInvoiceServiceSelection { get; set; }

    /// <summary>
    /// если true - организации могут создавать счета с одинаковыми л/с
    /// </summary>
    public bool AllowedHassameaccount { get; set; }

    /// <summary>
    /// за сколько дней до срока начинать напоминания по оплате
    /// </summary>
    public int Paymentreminderdaysbefore { get; set; }

    /// <summary>
    /// subscription или комбинация комиссий
    /// </summary>
    public string? BillingType { get; set; }

    /// <summary>
    /// нижняя комиссия от организации (от оборота)
    /// </summary>
    public bool UseLowerCommissionFromOrg { get; set; }

    /// <summary>
    /// FK на commission.id для нижней от организации
    /// </summary>
    public string? CommissionId { get; set; }

    /// <summary>
    /// верхняя комиссия от агента
    /// </summary>
    public bool UseUpperCommissionFromAgent { get; set; }

    /// <summary>
    /// нижняя комиссия к агенту
    /// </summary>
    public bool UseLowerCommissionToAgent { get; set; }

    public virtual Organization Organization { get; set; } = null!;
}
