using MenStyle.Web.Data;
using MenStyle.Web.Models;
using MenStyle.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenStyle.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        int productCount = await _context.Products
            .Where(p => p.IsActive)
            .CountAsync();

        int orderCount = await _context.CustomerOrders
            .CountAsync();

        int userCount = await _context.Users
            .CountAsync();

        int pendingOrderCount = await _context.CustomerOrders
            .Where(o => o.Status == "Chờ xác nhận")
            .CountAsync();

        decimal totalRevenue = await _context.CustomerOrders
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var recentOrders = await _context.CustomerOrders
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new OrderSummary
            {
                Code = o.OrderCode,
                CustomerName = o.CustomerName,
                Total = (o.TotalAmount.ToString("N0") + "đ").Replace(",", "."),
                Status = o.Status,
                StatusCssClass = GetStatusCssClass(o.Status)
            })
            .ToListAsync();

        var viewModel = new HomeViewModel
        {
            Categories = [],
            Products = [],

            Metrics =
            [
                new DashboardMetric
                {
                    Title = "Sản phẩm",
                    Value = productCount.ToString(),
                    Note = "Đang kinh doanh"
                },
                new DashboardMetric
                {
                    Title = "Đơn hàng",
                    Value = orderCount.ToString(),
                    Note = $"{pendingOrderCount} đơn chờ xác nhận"
                },
                new DashboardMetric
                {
                    Title = "Khách hàng",
                    Value = userCount.ToString(),
                    Note = "Tài khoản đã đăng ký"
                },
                new DashboardMetric
                {
                    Title = "Doanh thu",
                    Value = (totalRevenue.ToString("N0") + "đ").Replace(",", "."),
                    Note = "Tổng doanh thu đã ghi nhận"
                }
            ],

            RecentOrders = recentOrders
        };

        return View(viewModel);
    }

    public async Task<IActionResult> SanPham(string? category = "all", string? sort = "default")
    {
        var selectedCategory = string.IsNullOrWhiteSpace(category) ? "all" : category;
        var sortOrder = string.IsNullOrWhiteSpace(sort) ? "default" : sort;

        var categories = await _context.Categories
            .OrderBy(c => c.Id)
            .ToListAsync();

        foreach (var item in categories)
        {
            item.IsActive = item.Filter == selectedCategory;
        }

        var productQuery = _context.Products
            .Where(p => p.IsActive)
            .AsQueryable();

        if (selectedCategory != "all")
        {
            productQuery = productQuery
                .Where(p => p.CategorySlug == selectedCategory);
        }

        productQuery = sortOrder switch
        {
            "price-asc" => productQuery.OrderBy(p => p.Price),
            "price-desc" => productQuery.OrderByDescending(p => p.Price),
            _ => productQuery.OrderBy(p => p.Id)
        };

        var products = await productQuery.ToListAsync();

        var viewModel = new ProductListViewModel
        {
            Categories = categories,
            Products = products,
            SelectedCategory = selectedCategory,
            SortOrder = sortOrder
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    private static string GetStatusCssClass(string status)
    {
        return status switch
        {
            "Chờ xác nhận" => "pending",
            "Đang giao" => "shipping",
            "Hoàn thành" => "done",
            _ => "pending"
        };
    }
}