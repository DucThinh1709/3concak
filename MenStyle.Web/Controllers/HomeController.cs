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

    public IActionResult Index()
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

    // Trang sản phẩm riêng: /Home/SanPham
    // Lấy sản phẩm từ database, có lọc danh mục, sắp xếp giá và tìm kiếm
    public async Task<IActionResult> SanPham(string? category = "all", string? sort = "default", string? keyword = null)
    public async Task<IActionResult> SanPham(string? category = "all", string? sort = "default")
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

        var query = _context.Products.AsQueryable();
        var productQuery = _context.Products
            .Where(p => p.IsActive)
            .AsQueryable();

        // Lọc theo danh mục
        if (selectedCategory != "all")
        {
            query = query.Where(p => p.CategorySlug == selectedCategory);
        }

        // Tìm kiếm sản phẩm
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(p =>
                p.Name.Contains(searchKeyword) ||
                p.CategoryName.Contains(searchKeyword) ||
                p.CategorySlug.Contains(searchKeyword));
            productQuery = productQuery
                .Where(p => p.CategorySlug == selectedCategory);
        }

        // Sắp xếp sản phẩm
        query = sortOrder switch
        productQuery = sortOrder switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.Id)
            "price-asc" => productQuery.OrderBy(p => p.Price),
            "price-desc" => productQuery.OrderByDescending(p => p.Price),
            _ => productQuery.OrderBy(p => p.Id)
        };

        var products = await query.ToListAsync();

        var products = await productQuery.ToListAsync();

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
        return status switch
        {
            "Chờ xác nhận" => "pending",
            "Đang giao" => "shipping",
            "Hoàn thành" => "done",
            _ => "pending"
        };
    }
}