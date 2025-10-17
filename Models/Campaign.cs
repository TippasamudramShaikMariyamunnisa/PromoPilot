using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PromoPilot.Models
{
    [Table("Campaign")]
    public class Campaign
    {
        [Key]
        public int CampaignID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string TargetProducts { get; set; }

        [Required]
        public string StoreList { get; set; }

        // Navigation Properties
        public ICollection<Budget> Budgets { get; set; }
        public ICollection<ExecutionStatus> ExecutionStatuses { get; set; }
        public ICollection<Engagement> Engagements { get; set; }
        public ICollection<CampaignReport> CampaignReports { get; set; }
    }

}
