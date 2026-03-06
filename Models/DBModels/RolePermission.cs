using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class RolePermission
{
    public string Id { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Permission { get; set; } = null!;

    public int? Isdeleted { get; set; }

    public virtual Permission PermissionNavigation { get; set; } = null!;

    public virtual Role RoleNavigation { get; set; } = null!;
}
