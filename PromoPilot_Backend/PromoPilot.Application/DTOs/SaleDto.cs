using System;
using System.ComponentModel.DataAnnotations;

namespace PromoPilot.Application.DTOs
{
    public class SaleDto
    {
        // Assuming SaleId is auto-generated
        public int SaleId { get; set; }

        [Required(ErrorMessage = "Customer ID is required.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Product ID is required.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Campaign ID is required.")]
        public int CampaignId { get; set; }

        [Required(ErrorMessage = "Store ID is required.")]
        public int StoreId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Sale date is required.")]
        public DateTime SaleDate { get; set; }

        [Required(ErrorMessage = "Transaction ID is required.")]
        [StringLength(100, ErrorMessage = "Transaction ID can't be longer than 100 characters.")]
        public string? TransactionId { get; set; }

        [Required(ErrorMessage = "Payment method is required.")]
        [StringLength(50, ErrorMessage = "Payment method can't be longer than 50 characters.")]
        public string? PaymentMethod { get; set; }

        [Required(ErrorMessage = "Payment status is required.")]
        [StringLength(50, ErrorMessage = "Payment status can't be longer than 50 characters.")]
        public string? PaymentStatus { get; set; } 

        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; }
    }
}
