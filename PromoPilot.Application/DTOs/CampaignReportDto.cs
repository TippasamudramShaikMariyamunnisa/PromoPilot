using System;
using System.ComponentModel.DataAnnotations;

namespace PromoPilot.Application.DTOs
{
    public class CampaignReportDto
    {
        public int ReportID { get; set; }

        [Required(ErrorMessage = "Campaign ID is required.")]
        public int CampaignID { get; set; }

        [Required(ErrorMessage = "ROI is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "ROI must be a non-negative value.")]
        public decimal ROI { get; set; }

        [Required(ErrorMessage = "Reach is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Reach must be a non-negative number.")]
        public int Reach { get; set; }

        [Required(ErrorMessage = "Conversion rate is required.")]
        [Range(0.0, 100.0, ErrorMessage = "Conversion rate must be between 0 and 100.")]
        public decimal ConversionRate { get; set; }

        [Required(ErrorMessage = "Generated date is required.")]
        public DateTime GeneratedDate { get; set; }
    }
}
