using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class Organization
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    /// <summary>
    /// standart
    /// detsad
    /// school
    /// medclinic
    /// </summary>
    public string? Organizationtype { get; set; }

    public virtual ICollection<AgentCommission> AgentCommissions { get; } = new List<AgentCommission>();

    public virtual ICollection<OrgClientGroup> OrgClientGroups { get; } = new List<OrgClientGroup>();

    public virtual ICollection<OrganizationClient> OrganizationClients { get; } = new List<OrganizationClient>();

    public virtual ICollection<OrganizationField> OrganizationFields { get; } = new List<OrganizationField>();

    public virtual ICollection<OrganizationService> OrganizationServices { get; } = new List<OrganizationService>();

    public virtual OrganizationSetting? OrganizationSetting { get; set; }

    public virtual ICollection<Role> Roles { get; } = new List<Role>();

    public virtual ICollection<User> Users { get; } = new List<User>();
}
