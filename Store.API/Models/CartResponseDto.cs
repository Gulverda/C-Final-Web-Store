namespace Store.API.Models;

public class CartResponseDto
{
    public List<CartItemResponseDto> Items { get; set; } = new();
    public decimal GrandTotal { get; set; }
    public int TotalItems { get; set; }
}
