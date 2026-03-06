using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class OrganizationClient
{
    public string Id { get; set; } = null!;

    public string? Organization { get; set; }

    public string? ClientName { get; set; }

    /// <summary>
    /// fiz\jur
    /// </summary>
    public string? ClientType { get; set; }

    public string? ClientInn { get; set; }

    public string? ClientPhone { get; set; }

    public string? ClientAddress { get; set; }

    public string? ClientEmail { get; set; }

    /// <summary>
    /// баланс в тыйынах
    /// </summary>
    public decimal? ClientBalance { get; set; }

    /// <summary>
    /// 0\1
    /// </summary>
    public int? ClientStatus { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? UserCreater { get; set; }

    public string? ClientLogo { get; set; }

    /// <summary>
    /// ватсап клиента
    /// </summary>
    public string? ClientWa { get; set; }

    /// <summary>
    /// телеграмм клиента
    /// </summary>
    public string? ClientTg { get; set; }

    public string? OrgClientGroupId { get; set; }

    public virtual ICollection<Invoice> Invoices { get; } = new List<Invoice>();

    public virtual ICollection<Notification> Notifications { get; } = new List<Notification>();

    public virtual OrgClientGroup? OrgClientGroup { get; set; }

    public virtual ICollection<OrganizationClientsAdditionalField> OrganizationClientsAdditionalFields { get; } = new List<OrganizationClientsAdditionalField>();

    public virtual Organization? OrganizationNavigation { get; set; }

    public virtual User? UserCreaterNavigation { get; set; }
}
