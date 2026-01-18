namespace Store.Web.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ImageUrl { get; set; }
}

public class Cart
{
    public List<CartItem> Items { get; set; } = new();
    public decimal GrandTotal { get; set; }
    public int TotalItems { get; set; }
}

public class CartItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateQuantityDto
{
    public int Quantity { get; set; }
}
