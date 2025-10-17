using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PromoPilot.Models
{
    [Table("Engagement")]
    public class Engagement
    {
        [Key]
        public int EngagementID { get; set; }

        [Required]
        public int CampaignID { get; set; }

        [Required]
        public int CustomerID { get; set; }

        [Required]
        public int RedemptionCount { get; set; }

        [Required]
        public decimal PurchaseValue { get; set; }

        // Navigation Property
        public Campaign Campaign { get; set; }
    }

}
