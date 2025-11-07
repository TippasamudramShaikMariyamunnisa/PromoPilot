using System.ComponentModel.DataAnnotations;

public class EngagementDto
{
    public int EngagementID { get; set; }

    [Required(ErrorMessage = "Campaign ID is required.")]
    public int CampaignID { get; set; }

    [Required(ErrorMessage = "Customer ID is required.")]
    public int CustomerID { get; set; }

    [Required(ErrorMessage = "Redemption count is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Redemption count must be a non-negative number.")]
    public int RedemptionCount { get; set; }

    [Required(ErrorMessage = "Purchase value is required.")]
    [Range(0.0, double.MaxValue, ErrorMessage = "Purchase value must be a non-negative amount.")]
    public decimal PurchaseValue { get; set; }
}
