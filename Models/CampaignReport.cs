using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PromoPilot.Models
{
    [Table("CampaignReport")]
    public class CampaignReport
    {
        [Key]
        public int ReportID { get; set; } // ✅ This is the primary key

        [Required]
        public int CampaignID { get; set; }

        [Required]
        public decimal ROI { get; set; }

        [Required]
        public int Reach { get; set; }

        [Required]
        public decimal ConversionRate { get; set; }

        [Required]
        public DateTime GeneratedDate { get; set; }

        // Navigation Property
        public Campaign Campaign { get; set; }
    }


}
