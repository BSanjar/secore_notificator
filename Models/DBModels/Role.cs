using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class Role
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public int? Isdeleted { get; set; }

    /// <summary>
    /// права пользователей
    /// </summary>
    public string? Rights { get; set; }

    /// <summary>
    /// перечисляется id сервисов чз ;
    /// </summary>
    public string? AvilableServices { get; set; }

    public string? Organization { get; set; }

    public virtual Organization? OrganizationNavigation { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; } = new List<RolePermission>();

    public virtual ICollection<UserRole> UserRoles { get; } = new List<UserRole>();
}
