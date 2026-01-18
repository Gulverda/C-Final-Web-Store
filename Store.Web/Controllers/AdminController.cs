using Microsoft.AspNetCore.Mvc;
using Store.Web.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Store.Web.Controllers;

public class AdminController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IHttpClientFactory httpClientFactory, ILogger<AdminController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // GET: Admin
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

    // GET: Admin/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("StoreApi");
            var productDto = new
            {
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                ImageUrl = model.ImageUrl
            };

            var response = await client.PostAsJsonAsync("/api/products", productDto);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create product. Status: {Status}, Error: {Error}", response.StatusCode, errorContent);
                TempData["Error"] = "Failed to create product. Please try again.";
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            TempData["Error"] = "An error occurred while creating product.";
            return View(model);
        }
    }

    // GET: Admin/Edit/5
    public async Task<IActionResult> Edit(int? id)
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

                var viewModel = new ProductEditViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Description = product.Description,
                    ImageUrl = product.ImageUrl
                };

                return View(viewModel);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            else
            {
                _logger.LogError("Failed to fetch product {Id}. Status: {Status}", id, response.StatusCode);
                TempData["Error"] = "Failed to load product. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {Id}", id);
            TempData["Error"] = "An error occurred while loading product.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Admin/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("StoreApi");
            var productDto = new
            {
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                ImageUrl = model.ImageUrl
            };

            var response = await client.PutAsJsonAsync($"/api/products/{id}", productDto);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update product {Id}. Status: {Status}, Error: {Error}", id, response.StatusCode, errorContent);
                TempData["Error"] = "Failed to update product. Please try again.";
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            TempData["Error"] = "An error occurred while updating product.";
            return View(model);
        }
    }

    // GET: Admin/Delete/5
    public async Task<IActionResult> Delete(int? id)
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
                TempData["Error"] = "Failed to load product. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {Id}", id);
            TempData["Error"] = "An error occurred while loading product.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Admin/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("StoreApi");
            var response = await client.DeleteAsync($"/api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Product deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete product {Id}. Status: {Status}, Error: {Error}", id, response.StatusCode, errorContent);
                TempData["Error"] = "Failed to delete product. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            TempData["Error"] = "An error occurred while deleting product.";
            return RedirectToAction(nameof(Index));
        }
    }
}
