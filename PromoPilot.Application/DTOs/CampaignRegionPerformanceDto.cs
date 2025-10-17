using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoPilot.Application.DTOs
{
    public class CampaignRegionPerformanceDto
    {
        public string Region { get; set; } = string.Empty;
        public int CampaignID { get; set; }
        public decimal TotalROI { get; set; }
        public int TotalReach { get; set; }
        public decimal AverageConversionRate { get; set; }
    }
}
