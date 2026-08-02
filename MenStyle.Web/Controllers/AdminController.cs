using MenStyle.Web.Data;
using MenStyle.Web.Models;
using MenStyle.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenStyle.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private static readonly List<string> AllowedOrderStatuses =
    [
        "Chờ xác nhận",
        "Đang giao",
        "Hoàn thành",
        "Đã hủy"
    ];

    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var orders = await _context.CustomerOrders
            .AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var accountCount = await _context.Users.CountAsync();
        var activeProductCount = products.Count(p => p.IsActive);
        var pendingOrderCount = orders.Count(o => o.Status == "Chờ xác nhận");
        var completedRevenue = orders
            .Where(o => o.Status == "Hoàn thành")
            .Sum(o => o.TotalAmount);

        var viewModel = new AdminDashboardViewModel
        {
            Products = products,
            Orders = orders,
            OrderStatuses = AllowedOrderStatuses,
            Metrics =
            [
                new DashboardMetric
                {
                    Title = "Sản phẩm",
                    Value = products.Count.ToString(),
                    Note = $"{activeProductCount} sản phẩm đang hiển thị"
                },
                new DashboardMetric
                {
                    Title = "Đơn hàng",
                    Value = orders.Count.ToString(),
                    Note = $"{pendingOrderCount} đơn chờ xác nhận"
                },
                new DashboardMetric
                {
                    Title = "Tài khoản",
                    Value = accountCount.ToString(),
                    Note = "Tài khoản đã đăng ký"
                },
                new DashboardMetric
                {
                    Title = "Doanh thu",
                    Value = FormatPrice(completedRevenue),
                    Note = "Từ các đơn đã hoàn thành"
                }
            ]
        };

        return View(viewModel);
    }

    private static string FormatPrice(decimal price)
    {
        return $"{price:N0}đ".Replace(",", ".");
    }
}
