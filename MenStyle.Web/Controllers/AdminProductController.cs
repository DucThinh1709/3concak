using MenStyle.Web.Data;
using MenStyle.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MenStyle.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var defaultCategory = await _context.Categories
            .Where(c => c.Filter != "all")
            .OrderBy(c => c.Number)
            .FirstOrDefaultAsync();

        await LoadCategorySelectListAsync(defaultCategory?.Filter);
        await LoadProductTagSelectListAsync();
        return View(new Product
        {
            CategorySlug = defaultCategory?.Filter ?? "",
            CategoryName = defaultCategory?.Name ?? "",
            ImageUrl = "/images/product-tshirt.svg",
            Price = 0,
            OldPrice = 0,
            AvailableColors = string.Join(",", GenerateRandomColors(Environment.TickCount)),
            IsActive = true,
            CreatedAt = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model)
    {
        var category = await GetSelectedCategoryAsync(model.CategorySlug);

        if (category == null)
        {
            ModelState.AddModelError(nameof(model.CategorySlug), "Vui lòng chọn danh mục hợp lệ.");
        }
        else
        {
            model.CategorySlug = category.Filter;
            model.CategoryName = category.Name;

            ModelState.Remove(nameof(model.CategoryName));
        }

        ModelState.Remove(nameof(Product.ProductTags));
        model.ProductTags = model.ProductTags?.Trim() ?? "";

        if (!ModelState.IsValid)
        {
            await LoadCategorySelectListAsync(model.CategorySlug);
            await LoadProductTagSelectListAsync();
            return View(model);
        }

        model.Name = model.Name.Trim();
        model.ImageUrl = model.ImageUrl.Trim();
        model.AltText = model.AltText?.Trim() ?? "";
        model.ColorImageMap = model.ColorImageMap?.Trim() ?? "";
        model.CreatedAt = DateTime.Now;
        model.AvailableColors = model.AvailableColors?.Trim() ?? "";
        model.ProductTags = model.ProductTags?.Trim() ?? "";

        _context.Products.Add(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Thêm sản phẩm thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(product.AvailableColors))
        {
            product.AvailableColors = string.Join(",", GenerateRandomColors(product.Id));
        }

        await LoadCategorySelectListAsync(product.CategorySlug);
        await LoadProductTagSelectListAsync();

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var category = await GetSelectedCategoryAsync(model.CategorySlug);

        if (category == null)
        {
            ModelState.AddModelError(nameof(model.CategorySlug), "Vui lòng chọn danh mục hợp lệ.");
        }
        else
        {
            model.CategorySlug = category.Filter;
            model.CategoryName = category.Name;

            ModelState.Remove(nameof(model.CategoryName));
        }

        ModelState.Remove(nameof(Product.ProductTags));
        model.ProductTags = model.ProductTags?.Trim() ?? "";

        if (!ModelState.IsValid)
        {
            await LoadCategorySelectListAsync(model.CategorySlug);
            await LoadProductTagSelectListAsync();
            return View(model);
        }

        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        product.Name = model.Name.Trim();
        product.CategorySlug = model.CategorySlug;
        product.CategoryName = model.CategoryName;
        product.Price = model.Price;
        product.OldPrice = model.OldPrice;
        product.ImageUrl = model.ImageUrl.Trim();
        product.AltText = model.AltText?.Trim() ?? "";
        product.ColorImageMap = model.ColorImageMap?.Trim() ?? "";
        product.IsActive = model.IsActive;
        product.AvailableColors = model.AvailableColors?.Trim() ?? "";
        product.ProductTags = model.ProductTags?.Trim() ?? "";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Xóa sản phẩm thành công.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategorySelectListAsync(string? selectedCategorySlug = null)
    {
        var categories = await _context.Categories
            .Where(c => c.Filter != "all")
            .OrderBy(c => c.Number)
            .Select(c => new SelectListItem
            {
                Value = c.Filter,
                Text = c.Name,
                Selected = c.Filter == selectedCategorySlug
            })
            .ToListAsync();

        ViewBag.Categories = categories;
    }

    private async Task<Category?> GetSelectedCategoryAsync(string? categorySlug)
    {
        if (string.IsNullOrWhiteSpace(categorySlug))
        {
            return null;
        }

        var slug = categorySlug.Trim();

        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Filter == slug && c.Filter != "all");
    }
    private static List<string> GenerateRandomColors(int seed)
    {
        var colorPool = new List<string>
    {
        "Đen",
        "Trắng",
        "Xám",
        "Nâu",
        "Be",
        "Xanh navy",
        "Xanh rêu",
        "Xanh dương",
        "Đỏ đô",
        "Kem"
    };

        var random = new Random(seed);
        var colorCount = random.Next(1, 4);

        return colorPool
            .OrderBy(_ => random.Next())
            .Take(colorCount)
            .ToList();
    }
    private async Task LoadProductTagSelectListAsync()
    {
        var tags = await _context.Categories
            .Where(c => c.Filter != "all")
            .OrderBy(c => c.Number)
            .Select(c => new SelectListItem
            {
                Value = c.Filter,
                Text = c.Name
            })
            .ToListAsync();

        ViewBag.ProductTags = tags;
    }
}