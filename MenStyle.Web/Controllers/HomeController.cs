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

    public async Task<IActionResult> SanPham(string? category = "all", string? sort = "default", string? keyword = null)
    {
        var selectedCategory = string.IsNullOrWhiteSpace(category) ? "all" : category;
        var sortOrder = string.IsNullOrWhiteSpace(sort) ? "default" : sort;
        var searchKeyword = keyword?.Trim();

        var categories = await _context.Categories
            .OrderBy(c => c.Id)
            .ToListAsync();

        foreach (var item in categories)
        {
            item.IsActive = item.Filter == selectedCategory;
        }

        var query = _context.Products
            .Where(p => p.IsActive)
            .AsQueryable();

        if (selectedCategory != "all")
        {
            query = query.Where(p => p.CategorySlug == selectedCategory);
        }

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(p =>
                p.Name.Contains(searchKeyword) ||
                p.CategoryName.Contains(searchKeyword) ||
                p.CategorySlug.Contains(searchKeyword));
        }

        query = sortOrder switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.Id)
        };

        var products = await query.ToListAsync();

        var viewModel = new ProductListViewModel
        {
            Categories = categories,
            Products = products,
            SelectedCategory = selectedCategory,
            SortOrder = sortOrder,
            Keyword = searchKeyword
        };

        return View(viewModel);
    }

    public async Task<IActionResult> BoSuuTap()
    {
        var categories = await _context.Categories
            .Where(c => c.Filter != "all")
            .OrderBy(c => c.Id)
            .ToListAsync();

        var activeProducts = await _context.Products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        var collections = categories.Select(category => new CollectionItemViewModel
        {
            Title = category.Name,
            Slug = category.Filter,
            Description = category.Description,
            Products = activeProducts
                .Where(p => p.CategorySlug == category.Filter)
                .Take(4)
                .ToList()
        }).ToList();

        var viewModel = new CollectionPageViewModel
        {
            Collections = collections,
            FeaturedProducts = activeProducts.Take(6).ToList(),
            TotalProducts = activeProducts.Count
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