using Microsoft.AspNetCore.Mvc;
using Store.Web.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Store.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IHttpClientFactory httpClientFactory, ILogger<ProductsController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // GET: Products
    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("StoreApi");
            var response = await client.GetAsync("/api/products");

            if (response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadFromJsonAsync<List<Product>>();
                return View(products ?? new List<Product>());
            }
            else
            {
                _logger.LogError("Failed to fetch products. Status: {Status}", response.StatusCode);
                TempData["Error"] = "Failed to load products. Please try again later.";
                return View(new List<Product>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products");
            TempData["Error"] = "An error occurred while loading products.";
            return View(new List<Product>());
        }
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("StoreApi");
            var response = await client.GetAsync($"/api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadFromJsonAsync<Product>();
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            else
            {
                _logger.LogError("Failed to fetch product {Id}. Status: {Status}", id, response.StatusCode);
                TempData["Error"] = "Failed to load product details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {Id}", id);
            TempData["Error"] = "An error occurred while loading product details.";
            return RedirectToAction(nameof(Index));
        }
    }
}
