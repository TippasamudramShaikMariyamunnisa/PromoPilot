using System.ComponentModel.DataAnnotations.Schema;

namespace PromoPilot.Models
{
    [Table("Sale")]
    public class Sale
    {
        public int SaleID { get; set; }
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public int CampaignID { get; set; }
        public int StoreID { get; set; }

        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SaleDate { get; set; }

        // Merged transaction + payment fields
        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }

        public Customer Customer { get; set; }
        public Product Product { get; set; }
        public Campaign Campaign { get; set; }
    }

}
