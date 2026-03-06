using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class User
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Password { get; set; }

    public int? Isdeleted { get; set; }

    public string? Organization { get; set; }

    /// <summary>
    /// user
    /// admin
    /// superadmin
    /// </summary>
    public string? Role { get; set; }

    public string? GoogleId { get; set; }

    public string? GoogleEmail { get; set; }

    public bool? EmailConfirmed { get; set; }

    public bool? TwoFactorEnabled { get; set; }

    public string? TwoFactorType { get; set; }

    public string? TwoFactorSecret { get; set; }

    public string? TwoFactorCode { get; set; }

    public DateTime? TwoFactorCodeExpire { get; set; }

    public int? AccessFailedCount { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual ICollection<Invoice> Invoices { get; } = new List<Invoice>();

    public virtual ICollection<OrganizationClient> OrganizationClients { get; } = new List<OrganizationClient>();

    public virtual Organization? OrganizationNavigation { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; } = new List<UserRole>();
}
