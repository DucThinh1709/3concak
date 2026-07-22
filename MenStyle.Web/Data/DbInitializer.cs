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

        await db.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedCategoriesAsync(db);
        await SyncLegacyProductCategoriesAsync(db);
        await SeedProductsAsync(db);

        await db.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Customer"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
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

            return;
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext db)
    {
        await UpsertCategoryAsync(db, "00", "Tất cả", "all", "Toàn bộ sản phẩm", true);

        await UpsertCategoryAsync(db, "01", "Áo thun", "ao-thun", "Áo thun nam basic, oversize, dễ phối đồ");
        await UpsertCategoryAsync(db, "02", "Áo Polo", "ao-polo", "Áo polo nam lịch sự, dễ phối");
        await UpsertCategoryAsync(db, "03", "Áo sơ mi", "ao-so-mi", "Sơ mi nam công sở và casual");
        await UpsertCategoryAsync(db, "04", "Áo khoác", "ao-khoac", "Áo khoác nam, jacket, bomber");
        await UpsertCategoryAsync(db, "05", "Hoodie", "hoodie", "Áo hoodie nam trẻ trung, năng động");
        await UpsertCategoryAsync(db, "06", "Set đồ", "set-do", "Set đồ nam phối sẵn");

        await UpsertCategoryAsync(db, "07", "Quần nam", "quan-nam", "Tất cả các loại quần nam");
        await UpsertCategoryAsync(db, "08", "Quần Jean", "quan-jean", "Quần jean nam cá tính, dễ phối");
        await UpsertCategoryAsync(db, "09", "Quần Short", "quan-short", "Quần short nam thoải mái");
        await UpsertCategoryAsync(db, "10", "Quần Kaki & Chino", "quan-kaki-chino", "Quần kaki và chino nam");
        await UpsertCategoryAsync(db, "11", "Quần Jogger", "quan-jogger", "Quần jogger nam năng động");
        await UpsertCategoryAsync(db, "12", "Quần Tây", "quan-tay", "Quần tây nam lịch sự");

        await UpsertCategoryAsync(db, "13", "Basic dễ phối", "basic-de-phoi", "Sản phẩm basic dễ phối hằng ngày");
        await UpsertCategoryAsync(db, "14", "Oversize", "oversize", "Sản phẩm form rộng oversize");

        await db.SaveChangesAsync();

        await RemoveLegacyCategoryAsync(db, "so-mi");
        await RemoveLegacyCategoryAsync(db, "quan");

        await db.SaveChangesAsync();
    }

    private static async Task UpsertCategoryAsync(
        ApplicationDbContext db,
        string number,
        string name,
        string filter,
        string description,
        bool isActive = false)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Filter == filter);

        if (category == null)
        {
            db.Categories.Add(new Category
            {
                Number = number,
                Name = name,
                Filter = filter,
                Description = description,
                IsActive = isActive
            });

            return;
        }

        category.Number = number;
        category.Name = name;
        category.Filter = filter;
        category.Description = description;

        if (filter == "all")
        {
            category.IsActive = true;
        }
    }

    private static async Task RemoveLegacyCategoryAsync(ApplicationDbContext db, string oldFilter)
    {
        var oldCategory = await db.Categories
            .FirstOrDefaultAsync(c => c.Filter == oldFilter);

        if (oldCategory != null)
        {
            db.Categories.Remove(oldCategory);
        }
    }

    private static async Task SyncLegacyProductCategoriesAsync(ApplicationDbContext db)
    {
        var products = await db.Products.ToListAsync();

        foreach (var product in products)
        {
            var newSlug = NormalizeCategorySlug(product.CategorySlug);

            product.CategorySlug = newSlug;
            product.CategoryName = GetCategoryNameBySlug(newSlug);
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ApplicationDbContext db)
    {
        if (await db.Products.AnyAsync())
        {
            return;
        }

        var products = ProductCatalog.GetProducts();

        foreach (var item in products)
        {
            var categorySlug = NormalizeCategorySlug(item.CategorySlug);
            var categoryName = GetCategoryNameBySlug(categorySlug);

            db.Products.Add(new Product
            {
                Name = item.Name,
                CategorySlug = categorySlug,
                CategoryName = categoryName,
                Price = item.Price,
                OldPrice = item.OldPrice,
                ImageUrl = item.ImageUrl,
                AltText = item.AltText,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
        }

        await db.SaveChangesAsync();
    }

    private static string NormalizeCategorySlug(string? slug)
    {
        slug = slug?.Trim() ?? "";

        return slug switch
        {
            "so-mi" => "ao-so-mi",
            "quan" => "quan-nam",
            "ao-so-mi" => "ao-so-mi",
            "quan-nam" => "quan-nam",
            "ao-thun" => "ao-thun",
            "ao-polo" => "ao-polo",
            "ao-khoac" => "ao-khoac",
            "hoodie" => "hoodie",
            "set-do" => "set-do",
            "quan-jean" => "quan-jean",
            "quan-short" => "quan-short",
            "quan-kaki-chino" => "quan-kaki-chino",
            "quan-jogger" => "quan-jogger",
            "quan-tay" => "quan-tay",
            "basic-de-phoi" => "basic-de-phoi",
            "oversize" => "oversize",
            _ => "ao-thun"
        };
    }

    private static string GetCategoryNameBySlug(string slug)
    {
        return slug switch
        {
            "ao-thun" => "Áo thun",
            "ao-polo" => "Áo Polo",
            "ao-so-mi" => "Áo sơ mi",
            "ao-khoac" => "Áo khoác",
            "hoodie" => "Hoodie",
            "set-do" => "Set đồ",

            "quan-nam" => "Quần nam",
            "quan-jean" => "Quần Jean",
            "quan-short" => "Quần Short",
            "quan-kaki-chino" => "Quần Kaki & Chino",
            "quan-jogger" => "Quần Jogger",
            "quan-tay" => "Quần Tây",

            "basic-de-phoi" => "Basic dễ phối",
            "oversize" => "Oversize",

            _ => "Áo thun"
        };
    }
}