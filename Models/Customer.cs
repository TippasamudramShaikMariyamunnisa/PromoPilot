using System.ComponentModel.DataAnnotations.Schema;

namespace PromoPilot.Models
{
    [Table("Customer")]
    public class Customer
    {
        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public ICollection<Sale> Sales { get; set; }
    }

}
