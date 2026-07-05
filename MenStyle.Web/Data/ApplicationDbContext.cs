using MenStyle.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MenStyle.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
    public DbSet<CustomerOrderItem> CustomerOrderItems => Set<CustomerOrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Product>()
            .Property(p => p.OldPrice)
            .HasColumnType("decimal(18,2)");

        builder.Entity<CustomerOrder>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Entity<CustomerOrderItem>()
            .Property(i => i.Price)
            .HasColumnType("decimal(18,2)");

        builder.Entity<CustomerOrderItem>()
            .Property(i => i.LineTotal)
            .HasColumnType("decimal(18,2)");

        builder.Entity<CustomerOrder>()
            .HasMany(o => o.Items)
            .WithOne(i => i.CustomerOrder)
            .HasForeignKey(i => i.CustomerOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CustomerOrder>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Number = "01", Name = "Tất cả", Description = "Toàn bộ sản phẩm", Filter = "all", IsActive = true },
            new Category { Id = 2, Number = "02", Name = "Áo thun", Description = "Basic, oversize", Filter = "ao-thun", IsActive = false },
            new Category { Id = 3, Number = "03", Name = "Áo sơ mi", Description = "Công sở, casual", Filter = "so-mi", IsActive = false },
            new Category { Id = 4, Number = "04", Name = "Quần nam", Description = "Jeans, kaki", Filter = "quan", IsActive = false },
            new Category { Id = 5, Number = "05", Name = "Áo khoác", Description = "Bomber, denim", Filter = "ao-khoac", IsActive = false }
        );

        builder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                CategorySlug = "ao-thun",
                CategoryName = "Áo thun",
                Name = "Áo thun nam basic đen",
                Price = 249000,
                OldPrice = 320000,
                ImageUrl = "/images/product-tshirt.svg",
                AltText = "Áo thun nam basic đen",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Product
            {
                Id = 2,
                CategorySlug = "so-mi",
                CategoryName = "Sơ mi",
                Name = "Sơ mi Oxford trắng",
                Price = 399000,
                OldPrice = 450000,
                ImageUrl = "/images/product-shirt.svg",
                AltText = "Sơ mi Oxford trắng",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Product
            {
                Id = 3,
                CategorySlug = "quan",
                CategoryName = "Quần jeans",
                Name = "Quần jeans slim fit",
                Price = 459000,
                OldPrice = 520000,
                ImageUrl = "/images/product-jeans.svg",
                AltText = "Quần jeans slim fit",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Product
            {
                Id = 4,
                CategorySlug = "ao-khoac",
                CategoryName = "Áo khoác",
                Name = "Áo khoác bomber navy",
                Price = 599000,
                OldPrice = 690000,
                ImageUrl = "/images/product-jacket.svg",
                AltText = "Áo khoác bomber navy",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Product
            {
                Id = 5,
                CategorySlug = "quan",
                CategoryName = "Quần kaki",
                Name = "Quần kaki regular fit",
                Price = 379000,
                OldPrice = 430000,
                ImageUrl = "/images/product-kaki.svg",
                AltText = "Quần kaki regular fit",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Product
            {
                Id = 6,
                CategorySlug = "ao-thun",
                CategoryName = "Áo polo",
                Name = "Áo polo nam cao cấp",
                Price = 329000,
                OldPrice = 390000,
                ImageUrl = "/images/product-polo.svg",
                AltText = "Áo polo nam cao cấp",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }
}