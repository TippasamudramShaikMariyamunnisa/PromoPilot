using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PromoPilot.Models
{
    [Table("Budget")]
    public class Budget
    {
        [Key]
        public int BudgetID { get; set; }

        [Required]
        public int CampaignID { get; set; }

        [Required]
        public int StoreID { get; set; }

        [Required]
        public decimal AllocatedAmount { get; set; }

        [Required]
        public decimal SpentAmount { get; set; }

        // Navigation Property
        public Campaign Campaign { get; set; }
    }

}
