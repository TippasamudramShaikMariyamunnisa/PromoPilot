using System.ComponentModel.DataAnnotations;

public class CustomerDto
{
    // Assuming CustomerID is auto-generated in the database
    public int CustomerID { get; set; }

    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email { get; set; }
}
