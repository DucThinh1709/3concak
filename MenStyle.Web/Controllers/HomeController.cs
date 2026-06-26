using Microsoft.AspNetCore.Mvc;
using MenStyle.Web.Models;
using MenStyle.Web.ViewModels;

namespace MenStyle.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var viewModel = new HomeViewModel
        {
            Categories = GetCategories(),
            Products = GetProducts(),
            Metrics = GetMetrics(),
            RecentOrders = GetRecentOrders()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    private static List<Category> GetCategories()
    {
        return
        [
            new Category { Number = "01", Name = "Tất cả", Description = "Toàn bộ sản phẩm", Filter = "all", IsActive = true },
            new Category { Number = "02", Name = "Áo thun", Description = "Basic, oversize", Filter = "ao-thun" },
            new Category { Number = "03", Name = "Áo sơ mi", Description = "Công sở, casual", Filter = "so-mi" },
            new Category { Number = "04", Name = "Quần nam", Description = "Jeans, kaki", Filter = "quan" },
            new Category { Number = "05", Name = "Áo khoác", Description = "Bomber, denim", Filter = "ao-khoac" }
        ];
    }

    private static List<Product> GetProducts()
    {
        return
        [
            new Product { Id = 1, CategorySlug = "ao-thun", CategoryName = "Áo thun", Name = "Áo thun nam basic đen", Price = 249000, OldPrice = 320000, ImageUrl = "/images/product-tshirt.svg", AltText = "Áo thun nam basic đen" },
            new Product { Id = 2, CategorySlug = "so-mi", CategoryName = "Sơ mi", Name = "Sơ mi Oxford trắng", Price = 399000, OldPrice = 450000, ImageUrl = "/images/product-shirt.svg", AltText = "Sơ mi Oxford trắng" },
            new Product { Id = 3, CategorySlug = "quan", CategoryName = "Quần jeans", Name = "Quần jeans slim fit", Price = 459000, OldPrice = 520000, ImageUrl = "/images/product-jeans.svg", AltText = "Quần jeans slim fit" },
            new Product { Id = 4, CategorySlug = "ao-khoac", CategoryName = "Áo khoác", Name = "Áo khoác bomber navy", Price = 599000, OldPrice = 690000, ImageUrl = "/images/product-jacket.svg", AltText = "Áo khoác bomber navy" },
            new Product { Id = 5, CategorySlug = "quan", CategoryName = "Quần kaki", Name = "Quần kaki regular fit", Price = 379000, OldPrice = 430000, ImageUrl = "/images/product-kaki.svg", AltText = "Quần kaki regular fit" },
            new Product { Id = 6, CategorySlug = "ao-thun", CategoryName = "Áo polo", Name = "Áo polo nam cao cấp", Price = 329000, OldPrice = 390000, ImageUrl = "/images/product-polo.svg", AltText = "Áo polo nam cao cấp" }
        ];
    }

    private static List<DashboardMetric> GetMetrics()
    {
        return
        [
            new DashboardMetric { Title = "Sản phẩm", Value = "128", Note = "+12 sản phẩm mới" },
            new DashboardMetric { Title = "Đơn hàng", Value = "46", Note = "8 đơn chờ xác nhận" },
            new DashboardMetric { Title = "Khách hàng", Value = "312", Note = "24 khách hàng mới" },
            new DashboardMetric { Title = "Doanh thu", Value = "18.6M", Note = "Trong tháng này" }
        ];
    }

    private static List<OrderSummary> GetRecentOrders()
    {
        return
        [
            new OrderSummary { Code = "#MS1001", CustomerName = "Minh Quân", Total = "848.000đ", Status = "Chờ xác nhận", StatusCssClass = "pending" },
            new OrderSummary { Code = "#MS1002", CustomerName = "Hoàng Nam", Total = "599.000đ", Status = "Đang giao", StatusCssClass = "shipping" },
            new OrderSummary { Code = "#MS1003", CustomerName = "Đức Anh", Total = "1.087.000đ", Status = "Hoàn thành", StatusCssClass = "done" }
        ];
    }
}
