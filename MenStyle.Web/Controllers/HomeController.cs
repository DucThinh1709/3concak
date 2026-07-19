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

        var activeProducts = await _context.Products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        var randomProducts = activeProducts
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        var viewModel = new HomeViewModel
        {
            Categories = [],
            Products = [],

            HeroProduct = randomProducts.FirstOrDefault(),
            IntroProducts = randomProducts.Take(4).ToList(),
            SliderProducts = randomProducts.Take(10).ToList(),

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

    public async Task<IActionResult> SanPham(
        string? category = "all",
        string? sort = "default",
        string? keyword = null,
        bool saleOnly = false)
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
            .AsNoTracking()
            .Where(p => p.IsActive)
            .AsQueryable();

        if (selectedCategory != "all")
        {
            query = query.Where(p => p.CategorySlug == selectedCategory);
        }

        if (saleOnly)
        {
            query = query.Where(p => p.OldPrice > p.Price && p.OldPrice > 0);
        }

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(p =>
                p.Name.Contains(searchKeyword) ||
                p.CategoryName.Contains(searchKeyword) ||
                p.CategorySlug.Contains(searchKeyword));
        }

        var products = await query.ToListAsync();

        products = sortOrder switch
        {
            "price-asc" => products
                .OrderBy(p => p.Price)
                .ToList(),

            "price-desc" => products
                .OrderByDescending(p => p.Price)
                .ToList(),

            "discount-asc" => products
                .OrderBy(p => GetDiscountPercent(p.OldPrice, p.Price))
                .ToList(),

            "discount-desc" => products
                .OrderByDescending(p => GetDiscountPercent(p.OldPrice, p.Price))
                .ToList(),

            _ => products
                .OrderByDescending(p => p.Id)
                .ToList()
        };

        var viewModel = new ProductListViewModel
        {
            Categories = categories,
            Products = products,
            SelectedCategory = selectedCategory,
            SortOrder = sortOrder,
            Keyword = searchKeyword ?? "",
            SaleOnly = saleOnly
        };

        return View(viewModel);
    }

    public async Task<IActionResult> ChiTietSanPham(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
        {
            return NotFound();
        }

        var relatedProducts = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive
                        && p.Id != product.Id
                        && p.CategorySlug == product.CategorySlug)
            .OrderByDescending(p => p.Id)
            .Take(4)
            .ToListAsync();

        var colors = SplitOptions(product.AvailableColors);

        if (!colors.Any())
        {
            colors = GenerateRandomColors(product.Id);
        }

        var viewModel = new ProductDetailViewModel
        {
            Product = product,
            RelatedProducts = relatedProducts,
            Sizes = SplitOptions(product.AvailableSizes),
            Colors = colors,
            ColorImages = ParseColorImageMap(product.ColorImageMap)
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

    private static List<string> SplitOptions(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static List<string> GenerateRandomColors(int productId)
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

        var random = new Random(productId);

        return colorPool
            .OrderBy(_ => random.Next())
            .Take(4)
            .ToList();
    }

    private static Dictionary<string, string> ParseColorImageMap(string? value)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(value))
        {
            return result;
        }

        var pairs = value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2, StringSplitOptions.TrimEntries);

            if (parts.Length == 2
                && !string.IsNullOrWhiteSpace(parts[0])
                && !string.IsNullOrWhiteSpace(parts[1]))
            {
                result[parts[0]] = parts[1];
            }
        }

        return result;
    }

    private static int GetDiscountPercent(decimal oldPrice, decimal price)
    {
        if (oldPrice <= 0 || oldPrice <= price)
        {
            return 0;
        }

        return (int)Math.Round((double)((oldPrice - price) / oldPrice * 100));
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