using System;
using System.Collections.Generic;

namespace PromoPilot.Core.Entities
{
    public partial class Engagement
    {
        public int EngagementId { get; set; }

        public int CampaignId { get; set; }

        public int CustomerId { get; set; }

        public int RedemptionCount { get; set; }

        public decimal PurchaseValue { get; set; }

        public virtual Campaign Campaign { get; set; } = null!;
    }

}

