using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

/// <summary>
/// Связь агента и организации с видом комиссии (верхняя от агента / нижняя к агенту)
/// </summary>
public partial class AgentCommission
{
    public string Id { get; set; } = null!;

    public string AgentId { get; set; } = null!;

    public string OrganizationId { get; set; } = null!;

    public string CommissionId { get; set; } = null!;

    public string? LowerCommissionId { get; set; }

    public virtual Agent Agent { get; set; } = null!;

    public virtual Commission Commission { get; set; } = null!;

    public virtual Commission? LowerCommission { get; set; }

    public virtual Organization Organization { get; set; } = null!;
}
