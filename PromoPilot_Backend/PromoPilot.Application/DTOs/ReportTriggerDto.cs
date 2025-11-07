using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoPilot.Application.DTOs
{
    public class ReportTriggerDto
    {
        public string BlobName { get; set; } = string.Empty;
        public int StoreID { get; set; }
        public int CampaignID { get; set; }
    }

}
