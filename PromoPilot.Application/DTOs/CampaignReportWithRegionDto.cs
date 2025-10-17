using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoPilot.Application.DTOs
{
    public class CampaignReportWithRegionDto
    {
        public int CampaignID { get; set; }
        public decimal ROI { get; set; }
        public int Reach { get; set; }
        public decimal ConversionRate { get; set; }
        public string Region { get; set; } = string.Empty;
    }
}
