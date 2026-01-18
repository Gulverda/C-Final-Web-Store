using System.ComponentModel.DataAnnotations;

namespace Store.API.Models;

public class CreateOrderDto
{
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string SessionId { get; set; } = string.Empty;
}

public class OrderResponseDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemResponseDto> OrderItems { get; set; } = new();
}

public class OrderItemResponseDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
