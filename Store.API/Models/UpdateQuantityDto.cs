using System.ComponentModel.DataAnnotations;

namespace Store.API.Models;

public class UpdateQuantityDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}
