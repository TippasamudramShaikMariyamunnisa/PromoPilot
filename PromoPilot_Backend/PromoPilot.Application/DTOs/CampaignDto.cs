using System.ComponentModel.DataAnnotations;
using PromoPilot.Application.Validations;

namespace PromoPilot.Application.DTOs
{
    public class CampaignDto
    {
        public int CampaignId { get; set; }

        [Required(ErrorMessage = "Campaign name is required.")]
        [MaxLength(100, ErrorMessage = "Campaign name should not exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Campaign Start Date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Campaign End Date is required.")]
        [DataType(DataType.Date)]
        [DateGreaterThan("StartDate", ErrorMessage = "End Date must be after Start Date.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Target Products are required.")]
        [MinLength(3, ErrorMessage = "Target Products must be at least 3 characters.")]
        public string? TargetProducts { get; set; } = string.Empty;

        [Required(ErrorMessage = "Store List is required.")]
        [MinLength(3, ErrorMessage = "Store List must be at least 3 characters.")]
        public string? StoreList { get; set; } = string.Empty;
    }
}
