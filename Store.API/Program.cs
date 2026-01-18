using Microsoft.EntityFrameworkCore;
using Store.API.Data;
using Store.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlite("Data Source=Store.db"));

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

app.Run();
