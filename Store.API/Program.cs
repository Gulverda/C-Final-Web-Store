using Microsoft.EntityFrameworkCore;
using Store.API.Data;
using Store.API.Models;
using Store.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvcClient", policy =>
    {
        policy.WithOrigins("https://localhost:7124", "http://localhost:5192")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Database migrations will be handled manually via 'dotnet ef database update'
// Temporarily commented out to avoid conflicts during migration setup
/*
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<StoreDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("✅ Database and migrations applied successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ An error occurred applying migrations.");
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
    }
}
*/

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowMvcClient");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Products API Endpoints
app.MapGet("/api/products", async (StoreDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    return Results.Ok(products);
})
.WithName("GetProducts")
.Produces<List<Product>>(StatusCodes.Status200OK);

app.MapGet("/api/products/{id:int}", async (int id, StoreDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    
    if (product == null)
    {
        return Results.NotFound(new { message = $"Product with ID {id} not found." });
    }
    
    return Results.Ok(product);
})
.WithName("GetProductById")
.Produces<Product>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/products", async (CreateProductDto dto, StoreDbContext db) =>
{
    try
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return Results.Created($"/api/products/{product.Id}", product);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithName("CreateProduct")
.Produces<Product>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

app.MapPut("/api/products/{id:int}", async (int id, UpdateProductDto dto, StoreDbContext db) =>
{
    try
    {
        var product = await db.Products.FindAsync(id);
        
        if (product == null)
        {
            return Results.NotFound(new { message = $"Product with ID {id} not found." });
        }

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Description = dto.Description;
        product.ImageUrl = dto.ImageUrl;

        await db.SaveChangesAsync();

        return Results.Ok(product);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithName("UpdateProduct")
.Produces<Product>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

app.MapDelete("/api/products/{id:int}", async (int id, StoreDbContext db) =>
{
    try
    {
        var product = await db.Products.FindAsync(id);
        
        if (product == null)
        {
            return Results.NotFound(new { message = $"Product with ID {id} not found." });
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync();

        return Results.Ok(new { message = $"Product with ID {id} deleted successfully." });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithName("DeleteProduct")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// Cart API Endpoints
app.MapGet("/api/cart", async (string sessionId, ICartService cartService) =>
{
    try
    {
        var cart = await cartService.GetCartAsync(sessionId);
        return Results.Ok(cart);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithName("GetCart")
.Produces<CartResponseDto>(StatusCodes.Status200OK);

app.MapPost("/api/cart/items", async (string sessionId, CartItemDto item, ICartService cartService) =>
{
    try
    {
        var cartItem = await cartService.AddItemAsync(sessionId, item);
        return Results.Ok(cartItem);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithName("AddCartItem")
.Produces<CartItemResponseDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.MapPut("/api/cart/items/{productId:int}", async (string sessionId, int productId, UpdateQuantityDto dto, ICartService cartService) =>
{
    try
    {
        if (dto.Quantity <= 0)
        {
            return Results.BadRequest(new { message = "Quantity must be greater than 0." });
        }

        var cartItem = await cartService.UpdateItemQuantityAsync(sessionId, productId, dto.Quantity);
        return Results.Ok(cartItem);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithName("UpdateCartItem")
.Produces<CartItemResponseDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.MapDelete("/api/cart/items/{productId:int}", async (string sessionId, int productId, ICartService cartService) =>
{
    try
    {
        var removed = await cartService.RemoveItemAsync(sessionId, productId);
        if (removed)
        {
            return Results.Ok(new { message = "Item removed from cart." });
        }
        return Results.NotFound(new { message = "Item not found in cart." });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithName("RemoveCartItem")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// Orders API Endpoints
app.MapPost("/api/orders", async (CreateOrderDto dto, StoreDbContext db, ICartService cartService) =>
{
    try
    {
        // Get cart items
        var cart = await cartService.GetCartAsync(dto.SessionId);
        
        if (cart.Items == null || !cart.Items.Any())
        {
            return Results.BadRequest(new { message = "Cannot create order with empty cart." });
        }

        // Create order
        var order = new Order
        {
            CustomerName = dto.CustomerName,
            Address = dto.Address,
            Phone = dto.Phone,
            OrderDate = DateTime.UtcNow,
            TotalAmount = cart.GrandTotal,
            OrderItems = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = item.Price,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice
            }).ToList()
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Clear cart after order creation
        await cartService.ClearCartAsync(dto.SessionId);

        // Return order response
        var orderResponse = new OrderResponseDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            Address = order.Address,
            Phone = order.Phone,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                Price = oi.Price,
                Quantity = oi.Quantity,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };

        return Results.Ok(orderResponse);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithName("CreateOrder")
.Produces<OrderResponseDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.Run();