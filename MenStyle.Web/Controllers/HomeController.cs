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
            Categories = categories,
            Products = products,
            Metrics = new List<DashboardMetric>
        {
            new DashboardMetric
            {
                Title = "Sản phẩm",
                Value = products.Count.ToString(),
                Note = "Đang kinh doanh"
            },
            new DashboardMetric
            {
                Title = "Đơn hàng",
                Value = orderCount.ToString(),
                Note = "Tổng đơn đã tạo"
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
                Note = "Tổng doanh thu"
            }
        },
            RecentOrders = recentOrders
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