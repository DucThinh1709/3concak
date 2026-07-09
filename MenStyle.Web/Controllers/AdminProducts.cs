using MenStyle.Web.Data;
using MenStyle.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return View(products);
    }

    public IActionResult Create()
    {
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        product.Name = product.Name.Trim();
        product.CategoryName = product.CategoryName.Trim();
        product.CategorySlug = product.CategorySlug.Trim().ToLower();
        product.ImageUrl = product.ImageUrl.Trim();

        product.AltText = string.IsNullOrWhiteSpace(product.AltText)
            ? product.Name
            : product.AltText.Trim();

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Thêm sản phẩm thành công.";

        // Thêm xong quay về trang Sản phẩm để thấy sản phẩm mới
        return RedirectToAction("SanPham", "Home");
    }

    public async Task<IActionResult> Edit(int id)
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
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        var existingProduct = await _context.Products.FindAsync(id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        existingProduct.Name = product.Name.Trim();
        existingProduct.CategoryName = product.CategoryName.Trim();
        existingProduct.CategorySlug = product.CategorySlug.Trim().ToLower();
        existingProduct.Price = product.Price;
        existingProduct.OldPrice = product.OldPrice;
        existingProduct.ImageUrl = product.ImageUrl.Trim();
        existingProduct.AltText = string.IsNullOrWhiteSpace(product.AltText)
            ? product.Name.Trim()
            : product.AltText.Trim();

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công.";

        // Sửa xong quay về trang Sản phẩm để thấy thay đổi
        return RedirectToAction("SanPham", "Home");
    }

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

        // Xóa xong quay về trang Sản phẩm để cập nhật danh sách
        return RedirectToAction("SanPham", "Home");
    }
}