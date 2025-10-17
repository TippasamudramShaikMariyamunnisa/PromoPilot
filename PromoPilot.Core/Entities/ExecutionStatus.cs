using System;
using System.Collections.Generic;

namespace PromoPilot.Core.Entities;

public partial class ExecutionStatus
{
    public int StatusId { get; set; }

    public int CampaignId { get; set; }

    public int StoreId { get; set; }

    public string Status { get; set; } = null!;

    public string Feedback { get; set; } = null!;

    public virtual Campaign Campaign { get; set; } = null!;
}
