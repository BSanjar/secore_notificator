using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class OrgClientGroup
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? ParentGroupId { get; set; }

    public string OrganizationId { get; set; } = null!;

    /// <summary>
    /// 0 — активна, 1 — удалена (мягкое удаление)
    /// </summary>
    public int IsDeleted { get; set; }

    public string? Logo { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<OrgClientGroup> InverseParentGroup { get; } = new List<OrgClientGroup>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<OrganizationClient> OrganizationClients { get; } = new List<OrganizationClient>();

    public virtual OrgClientGroup? ParentGroup { get; set; }
}
