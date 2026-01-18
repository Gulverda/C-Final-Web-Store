using Store.API.Data;
using Store.API.Models;

namespace Store.API.Services;

public interface ICartService
{
    Task<CartResponseDto> GetCartAsync(string sessionId);
    Task<CartItemResponseDto> AddItemAsync(string sessionId, CartItemDto item);
    Task<CartItemResponseDto> UpdateItemQuantityAsync(string sessionId, int productId, int quantity);
    Task<bool> RemoveItemAsync(string sessionId, int productId);
    Task ClearCartAsync(string sessionId);
}

public class CartService : ICartService
{
    private readonly StoreDbContext _dbContext;
    private static readonly Dictionary<string, Dictionary<int, int>> _cartStore = new();

    public CartService(StoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CartResponseDto> GetCartAsync(string sessionId)
    {
        var cart = new CartResponseDto();

        if (_cartStore.TryGetValue(sessionId, out var items))
        {
            foreach (var item in items)
            {
                var product = await _dbContext.Products.FindAsync(item.Key);
                if (product != null)
                {
                    var cartItem = new CartItemResponseDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = item.Value,
                        TotalPrice = product.Price * item.Value,
                        ImageUrl = product.ImageUrl
                    };
                    cart.Items.Add(cartItem);
                    cart.GrandTotal += cartItem.TotalPrice;
                    cart.TotalItems += item.Value;
                }
            }
        }

        return cart;
    }

    public async Task<CartItemResponseDto> AddItemAsync(string sessionId, CartItemDto item)
    {
        var product = await _dbContext.Products.FindAsync(item.ProductId);
        if (product == null)
        {
            throw new ArgumentException($"Product with ID {item.ProductId} not found.");
        }

        if (!_cartStore.ContainsKey(sessionId))
        {
            _cartStore[sessionId] = new Dictionary<int, int>();
        }

        if (_cartStore[sessionId].ContainsKey(item.ProductId))
        {
            _cartStore[sessionId][item.ProductId] += item.Quantity;
        }
        else
        {
            _cartStore[sessionId][item.ProductId] = item.Quantity;
        }

        return new CartItemResponseDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Price = product.Price,
            Quantity = _cartStore[sessionId][item.ProductId],
            TotalPrice = product.Price * _cartStore[sessionId][item.ProductId],
            ImageUrl = product.ImageUrl
        };
    }

    public async Task<CartItemResponseDto> UpdateItemQuantityAsync(string sessionId, int productId, int quantity)
    {
        var product = await _dbContext.Products.FindAsync(productId);
        if (product == null)
        {
            throw new ArgumentException($"Product with ID {productId} not found.");
        }

        if (!_cartStore.ContainsKey(sessionId) || !_cartStore[sessionId].ContainsKey(productId))
        {
            throw new ArgumentException($"Cart item for Product ID {productId} not found.");
        }

        _cartStore[sessionId][productId] = quantity;

        return new CartItemResponseDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Price = product.Price,
            Quantity = quantity,
            TotalPrice = product.Price * quantity,
            ImageUrl = product.ImageUrl
        };
    }

    public Task<bool> RemoveItemAsync(string sessionId, int productId)
    {
        if (_cartStore.ContainsKey(sessionId) && _cartStore[sessionId].ContainsKey(productId))
        {
            _cartStore[sessionId].Remove(productId);
            
            // Clean up empty cart
            if (_cartStore[sessionId].Count == 0)
            {
                _cartStore.Remove(sessionId);
            }
            
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task ClearCartAsync(string sessionId)
    {
        if (_cartStore.ContainsKey(sessionId))
        {
            _cartStore.Remove(sessionId);
        }
        return Task.CompletedTask;
    }
}
