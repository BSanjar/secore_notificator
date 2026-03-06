using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class Agent
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? ApiLogin { get; set; }

    public string? ApiPsw { get; set; }

    public virtual ICollection<AgentCommission> AgentCommissions { get; } = new List<AgentCommission>();

    public virtual ICollection<Transaction> Transactions { get; } = new List<Transaction>();
}
