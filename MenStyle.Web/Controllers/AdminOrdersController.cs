using MenStyle.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenStyle.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminOrdersController : Controller
{
    private static readonly string[] AllowedStatuses =
    [
        "Chờ xác nhận",
        "Đang giao",
        "Hoàn thành",
        "Đã hủy"
    ];

    private readonly ApplicationDbContext _context;

    public AdminOrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return Redirect("/Admin#orders");
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.CustomerOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        ViewBag.Statuses = AllowedStatuses.ToList();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? returnUrl = null)
    {
        var order = await _context.CustomerOrders.FindAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        if (!AllowedStatuses.Contains(status))
        {
            TempData["ErrorMessage"] = "Trạng thái đơn hàng không hợp lệ.";
            return RedirectAfterUpdate(id, returnUrl);
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công.";
        return RedirectAfterUpdate(id, returnUrl);
    }

    private IActionResult RedirectAfterUpdate(int id, string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
