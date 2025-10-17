using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PromoPilot.Models
{
    [Table("ExecutionStatus")]
    public class ExecutionStatus
    {
        [Key]
        public int StatusID { get; set; }

        [Required]
        public int CampaignID { get; set; }

        [Required]
        public int StoreID { get; set; }

        [Required]
        public string Status { get; set; }

        public string Feedback { get; set; }

        // Navigation Property
        public Campaign Campaign { get; set; }
    }

}
