using MenStyle.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MenStyle.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = ["Admin", "Customer"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        string adminEmail = "admin@menstyle.vn";
        string adminPassword = "Admin@123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Quản trị viên MENSTYLE",
                PhoneNumber = "0909123456",
                Address = "TP. Hồ Chí Minh",
                Gender = "Nam",
                CreatedAt = DateTime.Now
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Number = "01", Name = "Tất cả", Description = "Toàn bộ sản phẩm", Filter = "all", IsActive = true },
                new Category { Number = "02", Name = "Áo thun", Description = "Basic, oversize", Filter = "ao-thun", IsActive = false },
                new Category { Number = "03", Name = "Áo sơ mi", Description = "Công sở, casual", Filter = "so-mi", IsActive = false },
                new Category { Number = "04", Name = "Quần nam", Description = "Jeans, kaki", Filter = "quan", IsActive = false },
                new Category { Number = "05", Name = "Áo khoác", Description = "Bomber, denim", Filter = "ao-khoac", IsActive = false }
            );
        }

        if (!await db.Products.AnyAsync())
        {
            var products = ProductCatalog.GetProducts();

            foreach (var item in products)
            {
                db.Products.Add(new Product
                {
                    Name = item.Name,
                    CategorySlug = item.CategorySlug,
                    CategoryName = item.CategoryName,
                    Price = item.Price,
                    OldPrice = item.OldPrice,
                    ImageUrl = item.ImageUrl,
                    AltText = item.AltText,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }
        }

        await db.SaveChangesAsync();
    }
}