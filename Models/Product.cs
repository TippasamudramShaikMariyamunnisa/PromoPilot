using System.ComponentModel.DataAnnotations.Schema;

namespace PromoPilot.Models
{
    [Table("Product")]
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }

        public ICollection<Sale> Sales { get; set; }
    }

}
