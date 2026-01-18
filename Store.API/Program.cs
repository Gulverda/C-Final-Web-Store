using Microsoft.EntityFrameworkCore;
using Store.API.Data;
using Store.API.Models;
using Store.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlite("Data Source=Store.db"));

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

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
    dbContext.Database.EnsureCreated();
}

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

app.Run();
