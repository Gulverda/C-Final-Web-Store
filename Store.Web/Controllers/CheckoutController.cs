using Microsoft.AspNetCore.Mvc;
using Store.Web.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Store.Web.Controllers;

public class CheckoutController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(IHttpClientFactory httpClientFactory, ILogger<CheckoutController> logger)
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

    // GET: Checkout
    [HttpGet]
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
                if (cart == null || !cart.Items.Any())
                {
                    TempData["Error"] = "Your cart is empty. Please add items to your cart before checkout.";
                    return RedirectToAction("Index", "Cart");
                }

                var viewModel = new CheckoutViewModel
                {
                    Cart = cart
                };

                return View(viewModel);
            }
            else
            {
                _logger.LogError("Failed to fetch cart. Status: {Status}", response.StatusCode);
                TempData["Error"] = "Failed to load cart. Please try again later.";
                return RedirectToAction("Index", "Cart");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading checkout");
            TempData["Error"] = "An error occurred while loading checkout.";
            return RedirectToAction("Index", "Cart");
        }
    }

    // POST: Checkout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload cart to display errors
            try
            {
                var sessionId = GetSessionId();
                var client = _httpClientFactory.CreateClient("StoreApi");
                var response = await client.GetAsync($"/api/cart?sessionId={Uri.EscapeDataString(sessionId)}");

                if (response.IsSuccessStatusCode)
                {
                    var cart = await response.Content.ReadFromJsonAsync<Cart>();
                    model.Cart = cart ?? new Cart();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading cart during validation");
            }

            return View(model);
        }

        try
        {
            var sessionId = GetSessionId();
            var client = _httpClientFactory.CreateClient("StoreApi");

            var createOrderDto = new
            {
                CustomerName = model.CustomerName,
                Address = model.Address,
                Phone = model.Phone,
                SessionId = sessionId
            };

            var response = await client.PostAsJsonAsync("/api/orders", createOrderDto);

            if (response.IsSuccessStatusCode)
            {
                var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
                
                if (order != null)
                {
                    TempData["Success"] = $"Order #{order.Id} placed successfully! Thank you for your purchase.";
                    TempData["OrderId"] = order.Id;
                    return RedirectToAction("Success", new { orderId = order.Id });
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create order. Status: {Status}, Error: {Error}", response.StatusCode, errorContent);
                
                // Try to parse error message
                try
                {
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorObj.TryGetProperty("message", out var messageElement))
                    {
                        ModelState.AddModelError("", messageElement.GetString() ?? "Failed to create order. Please try again.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create order. Please try again.");
                    }
                }
                catch
                {
                    ModelState.AddModelError("", "Failed to create order. Please try again.");
                }

                // Reload cart
                try
                {
                    var cartResponse = await client.GetAsync($"/api/cart?sessionId={Uri.EscapeDataString(sessionId)}");
                    if (cartResponse.IsSuccessStatusCode)
                    {
                        var cart = await cartResponse.Content.ReadFromJsonAsync<Cart>();
                        model.Cart = cart ?? new Cart();
                    }
                }
                catch { }

                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            ModelState.AddModelError("", "An error occurred while processing your order. Please try again.");

            // Reload cart
            try
            {
                var sessionId = GetSessionId();
                var client = _httpClientFactory.CreateClient("StoreApi");
                var cartResponse = await client.GetAsync($"/api/cart?sessionId={Uri.EscapeDataString(sessionId)}");
                if (cartResponse.IsSuccessStatusCode)
                {
                    var cart = await cartResponse.Content.ReadFromJsonAsync<Cart>();
                    model.Cart = cart ?? new Cart();
                }
            }
            catch { }

            return View(model);
        }

        return View(model);
    }

    // GET: Checkout/Success
    [HttpGet]
    public IActionResult Success(int? orderId)
    {
        if (orderId == null)
        {
            TempData["Error"] = "Invalid order ID.";
            return RedirectToAction("Index", "Home");
        }

        ViewBag.OrderId = orderId;
        return View();
    }
}
