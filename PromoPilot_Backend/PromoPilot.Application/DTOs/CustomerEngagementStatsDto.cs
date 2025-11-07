using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoPilot.Application.DTOs
{
    public class CustomerEngagementStatsDto
    {
        public int CustomerID { get; set; }
        public int CampaignID { get; set; }
        public int TotalRedemptions { get; set; }
        public decimal TotalPurchaseValue { get; set; }
        public int VisitCount { get; set; }
    }

}
