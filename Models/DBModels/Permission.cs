using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class Permission
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? Code { get; set; }

    public string? Description { get; set; }

    public string? Category { get; set; }

    public int? Isdeleted { get; set; }

    public string? Area { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; } = new List<RolePermission>();
}
