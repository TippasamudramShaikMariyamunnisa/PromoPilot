using System;
using System.Collections.Generic;

namespace PromoPilot.Core.Entities;

public partial class Budget
{
    public int BudgetId { get; set; }

    public int CampaignId { get; set; }

    public int StoreId { get; set; }

    public decimal AllocatedAmount { get; set; }

    public decimal SpentAmount { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;
}
