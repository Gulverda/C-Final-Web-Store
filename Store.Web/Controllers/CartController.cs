using Microsoft.AspNetCore.Mvc;
using Store.Web.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Store.Web.Controllers;

public class CartController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CartController> _logger;

    public CartController(IHttpClientFactory httpClientFactory, ILogger<CartController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private string GetSessionId()
    {
        var sessionId = HttpContext.Session.GetString("CartSessionId");
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("CartSessionId", sessionId);
        }
        return sessionId;
    }

    // GET: Cart
    public async Task<IActionResult> Index()
    {
        try
        {
            var sessionId = GetSessionId();
            var client = _httpClientFactory.CreateClient("StoreApi");
            var response = await client.GetAsync($"/api/cart?sessionId={Uri.EscapeDataString(sessionId)}");

            if (response.IsSuccessStatusCode)
            {
                var cart = await response.Content.ReadFromJsonAsync<Cart>();
                return View(cart ?? new Cart());
            }
            else
            {
                _logger.LogError("Failed to fetch cart. Status: {Status}", response.StatusCode);
                TempData["Error"] = "Failed to load cart. Please try again later.";
                return View(new Cart());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cart");
            TempData["Error"] = "An error occurred while loading your cart.";
            return View(new Cart());
        }
    }

    // POST: Cart/AddItem
    [HttpPost]
    public async Task<IActionResult> AddItem(int productId, int quantity = 1)
    {
        try
        {
            var sessionId = GetSessionId();
            var client = _httpClientFactory.CreateClient("StoreApi");
            var item = new CartItemDto { ProductId = productId, Quantity = quantity };
            
            var response = await client.PostAsJsonAsync($"/api/cart/items?sessionId={Uri.EscapeDataString(sessionId)}", item);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Item added to cart successfully!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to add item to cart. Status: {Status}, Error: {Error}", response.StatusCode, errorContent);
                TempData["Error"] = "Failed to add item to cart. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart");
            TempData["Error"] = "An error occurred while adding item to cart.";
        }

        return RedirectToAction(nameof(Index));
    }

    // PUT: Cart/UpdateQuantity
    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
    {
        try
        {
            var sessionId = GetSessionId();
            var client = _httpClientFactory.CreateClient("StoreApi");
            var dto = new UpdateQuantityDto { Quantity = quantity };
            
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/api/cart/items/{productId}?sessionId={Uri.EscapeDataString(sessionId)}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cart updated successfully!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update cart item. Status: {Status}, Error: {Error}", response.StatusCode, errorContent);
                TempData["Error"] = "Failed to update cart item. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item");
            TempData["Error"] = "An error occurred while updating cart item.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Cart/RemoveItem
    [HttpPost]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        try
        {
            var sessionId = GetSessionId();
            var client = _httpClientFactory.CreateClient("StoreApi");
            var response = await client.DeleteAsync($"/api/cart/items/{productId}?sessionId={Uri.EscapeDataString(sessionId)}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Item removed from cart successfully!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to remove item from cart. Status: {Status}, Error: {Error}", response.StatusCode, errorContent);
                TempData["Error"] = "Failed to remove item from cart. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from cart");
            TempData["Error"] = "An error occurred while removing item from cart.";
        }

        return RedirectToAction(nameof(Index));
    }
}
