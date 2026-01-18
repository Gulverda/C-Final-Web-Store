using Microsoft.EntityFrameworkCore;
using Store.API.Models;

namespace Store.API.Data;

public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed initial data
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Wireless Mouse",
                Price = 29.99m,
                Description = "Ergonomic wireless mouse with high precision sensor.",
                ImageUrl = "https://images.unsplash.com/photo-1527814050087-3793815479db?w=400"
            },
            new Product
            {
                Id = 2,
                Name = "Mechanical Keyboard",
                Price = 89.99m,
                Description = "RGB backlit mechanical keyboard with cherry MX switches.",
                ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400"
            },
            new Product
            {
                Id = 3,
                Name = "USB-C Hub",
                Price = 45.99m,
                Description = "Multi-port USB-C hub with HDMI and SD card support.",
                ImageUrl = "https://images.unsplash.com/photo-1625842268584-8f3296236761?w=400"
            },
            new Product
            {
                Id = 4,
                Name = "Laptop Stand",
                Price = 34.99m,
                Description = "Adjustable aluminum laptop stand for better ergonomics.",
                ImageUrl = "https://images.unsplash.com/photo-1593541937377-c7d0e0f8bf9a?w=400"
            },
            new Product
            {
                Id = 5,
                Name = "Wireless Headphones",
                Price = 129.99m,
                Description = "Noise-cancelling wireless headphones with 30-hour battery.",
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400"
            }
        );

        // Configure Order-OrderItem relationship
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
