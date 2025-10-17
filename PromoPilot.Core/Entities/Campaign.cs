using System;
using System.Collections.Generic;

namespace PromoPilot.Core.Entities;

public partial class Campaign
{
    public int CampaignId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string TargetProducts { get; set; } = null!;

    public string StoreList { get; set; } = null!;

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<CampaignReport> CampaignReports { get; set; } = new List<CampaignReport>();

    public virtual ICollection<Engagement> Engagements { get; set; } = new List<Engagement>();

    public virtual ICollection<ExecutionStatus> ExecutionStatuses { get; set; } = new List<ExecutionStatus>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
