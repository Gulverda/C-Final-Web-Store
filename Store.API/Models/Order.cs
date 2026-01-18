using System.ComponentModel.DataAnnotations;

namespace Store.API.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    // Navigation property
    public List<OrderItem> OrderItems { get; set; } = new();
}
