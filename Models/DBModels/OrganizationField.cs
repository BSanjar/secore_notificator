using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class OrganizationField
{
    public string Id { get; set; } = null!;

    public string? FieldName { get; set; }

    /// <summary>
    /// int
    /// string
    /// money
    /// selected
    /// datetime
    /// </summary>
    public string? FieldType { get; set; }

    /// <summary>
    /// варианты для выбора чз - ;
    /// </summary>
    public string? FieldSelectValues { get; set; }

    public int? Isdeleted { get; set; }

    public string? Organization { get; set; }

    public bool? Filterbyfield { get; set; }

    public virtual ICollection<OrganizationClientsAdditionalField> OrganizationClientsAdditionalFields { get; } = new List<OrganizationClientsAdditionalField>();

    public virtual Organization? OrganizationNavigation { get; set; }
}
