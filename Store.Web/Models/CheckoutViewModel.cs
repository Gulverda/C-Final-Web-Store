using System.ComponentModel.DataAnnotations;

namespace Store.Web.Models;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    public Cart Cart { get; set; } = new();
}

public class OrderResponse
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new();
}

public class OrderItemResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
