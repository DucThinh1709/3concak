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
        var categories = await _context.Categories
            .OrderBy(c => c.Id)
            .ToListAsync();

        var products = await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Id)
            .ToListAsync();

        int orderCount = await _context.CustomerOrders.CountAsync();
        int userCount = await _context.Users.CountAsync();

        var recentOrders = await _context.CustomerOrders
            .OrderByDescending(o => o.CreatedAt)
            .Take(3)
            .Select(o => new OrderSummary
            {
                Code = o.OrderCode,
                CustomerName = o.CustomerName,
                Total = (o.TotalAmount.ToString("N0") + "đ").Replace(",", "."),
                Status = o.Status,
                StatusCssClass = GetStatusCssClass(o.Status)
            })
            .ToListAsync();

        var totalRevenue = await _context.CustomerOrders
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var viewModel = new HomeViewModel
        {
            // Trang chủ không còn hiển thị sản phẩm nữa
            Categories = [],
            Products = [],

            Metrics = GetMetrics(),
            RecentOrders = GetRecentOrders()
        };

        return View(viewModel);
    }

    // Trang sản phẩm riêng: /Home/SanPham
    public IActionResult SanPham(string? category = "all", string? sort = "default")
    {
        var selectedCategory = string.IsNullOrWhiteSpace(category) ? "all" : category;
        var sortOrder = string.IsNullOrWhiteSpace(sort) ? "default" : sort;

        var categories = GetCategories();

        foreach (var item in categories)
        {
            item.IsActive = item.Filter == selectedCategory;
        }

        var products = GetProducts();

        if (selectedCategory != "all")
        {
            products = products
                .Where(p => p.CategorySlug == selectedCategory)
                .ToList();
        }

        products = sortOrder switch
        {
            "price-asc" => products.OrderBy(p => p.Price).ToList(),
            "price-desc" => products.OrderByDescending(p => p.Price).ToList(),
            _ => products
        };

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
        return
        [
            new Category
            {
                Number = "01",
                Name = "Tất cả",
                Description = "Toàn bộ sản phẩm",
                Filter = "all",
                IsActive = true
            },
            new Category
            {
                Number = "02",
                Name = "Áo thun",
                Description = "Basic, oversize",
                Filter = "ao-thun"
            },
            new Category
            {
                Number = "03",
                Name = "Áo sơ mi",
                Description = "Công sở, casual",
                Filter = "so-mi"
            },
            new Category
            {
                Number = "04",
                Name = "Quần nam",
                Description = "Jeans, kaki",
                Filter = "quan"
            },
            new Category
            {
                Number = "05",
                Name = "Áo khoác",
                Description = "Bomber, denim",
                Filter = "ao-khoac"
            }
        ];
    }

    private static List<Product> GetProducts()
    {
        return ProductCatalog.GetProducts();
    }

    private static List<DashboardMetric> GetMetrics()
    {
        return
        [
            new DashboardMetric
            {
                Title = "Sản phẩm",
                Value = "128",
                Note = "+12 sản phẩm mới"
            },
            new DashboardMetric
            {
                Title = "Đơn hàng",
                Value = "46",
                Note = "8 đơn chờ xác nhận"
            },
            new DashboardMetric
            {
                Title = "Khách hàng",
                Value = "312",
                Note = "24 khách hàng mới"
            },
            new DashboardMetric
            {
                Title = "Doanh thu",
                Value = "18.6M",
                Note = "Trong tháng này"
            }
        ];
    }

    private static List<OrderSummary> GetRecentOrders()
    {
        return
        [
            new OrderSummary
            {
                Code = "#MS1001",
                CustomerName = "Minh Quân",
                Total = "848.000đ",
                Status = "Chờ xác nhận",
                StatusCssClass = "pending"
            },
            new OrderSummary
            {
                Code = "#MS1002",
                CustomerName = "Hoàng Nam",
                Total = "599.000đ",
                Status = "Đang giao",
                StatusCssClass = "shipping"
            },
            new OrderSummary
            {
                Code = "#MS1003",
                CustomerName = "Đức Anh",
                Total = "1.087.000đ",
                Status = "Hoàn thành",
                StatusCssClass = "done"
            }
        ];
    }
}