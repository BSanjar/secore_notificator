using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class OrganizationClientsAdditionalField
{
    public string Id { get; set; } = null!;

    /// <summary>
    /// ссылка на organization_fields
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    /// значение переменной\поля
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// ссылка на клиента
    /// </summary>
    public string? OrganizationClient { get; set; }

    public virtual OrganizationField? FieldNavigation { get; set; }

    public virtual OrganizationClient? OrganizationClientNavigation { get; set; }
}
