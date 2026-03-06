using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class CommissionTier
{
    public string Id { get; set; } = null!;

    public string CommissionId { get; set; } = null!;

    public decimal AmountFrom { get; set; }

    public decimal AmountTo { get; set; }

    public decimal Rate { get; set; }

    public int SortOrder { get; set; }

    public virtual Commission Commission { get; set; } = null!;
}
