using System.ComponentModel.DataAnnotations;

public class ProductDto
{
    // Assuming ProductID is auto-generated
    public int ProductID { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(100, ErrorMessage = "Product name can't be longer than 100 characters.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(50, ErrorMessage = "Category can't be longer than 50 characters.")]
    public string? Category { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
}
