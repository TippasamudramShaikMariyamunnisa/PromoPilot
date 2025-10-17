using System.ComponentModel.DataAnnotations;

namespace PromoPilot.Application.DTOs
{
    public class BudgetDto
    {
        public int BudgetID { get; set; }

        [Required(ErrorMessage = "CampaignID is required")]
        public int? CampaignID { get; set; }

        [Required(ErrorMessage = "StoreID is required")]
        public int StoreID { get; set; }

        [Required(ErrorMessage = "AllocatedAmount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "AllocatedAmount must be greater than zero")]
        public decimal AllocatedAmount { get; set; }

        [Required(ErrorMessage = "SpentAmount is required")]
        [Range(0.00, double.MaxValue, ErrorMessage = "SpentAmount cannot be negative")]
        public decimal SpentAmount { get; set; }
    }
}
