using MenStyle.Web.Data;
using MenStyle.Web.Extensions;
using MenStyle.Web.Models;
using MenStyle.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenStyle.Web.Controllers;

public class CartController : Controller
{
    private const string CartSessionKey = "MENSTYLE_CART";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var cart = GetCart();

        var viewModel = new CartViewModel
        {
            Items = cart
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id, string? returnUrl = null)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
        {
            return NotFound();
        }

        var cart = GetCart();

        var existingItem = cart.FirstOrDefault(x => x.ProductId == product.Id);

        if (existingItem == null)
        {
            cart.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CategoryName = product.CategoryName,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                Quantity = 1
            });
        }
        else
        {
            existingItem.Quantity++;
        }

        SaveCart(cart);

        TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Increase(int id)
    {
        var cart = GetCart();

        var item = cart.FirstOrDefault(x => x.ProductId == id);

        if (item != null)
        {
            item.Quantity++;
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Decrease(int id)
    {
        var cart = GetCart();

        var item = cart.FirstOrDefault(x => x.ProductId == id);

        if (item != null)
        {
            item.Quantity--;

            if (item.Quantity <= 0)
            {
                cart.Remove(item);
            }

            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int id)
    {
        var cart = GetCart();

        var item = cart.FirstOrDefault(x => x.ProductId == id);

        if (item != null)
        {
            cart.Remove(item);
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        HttpContext.Session.Remove(CartSessionKey);

        TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng.";

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var cart = GetCart();

        if (!cart.Any())
        {
            TempData["ErrorMessage"] = "Giỏ hàng đang trống, chưa thể đặt hàng.";
            return RedirectToAction("Index");
        }

        ApplicationUser? user = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            user = await _userManager.GetUserAsync(User);
        }

        var order = new CustomerOrder
        {
            OrderCode = "MS" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            UserId = user?.Id,
            CustomerName = user?.FullName ?? "Khách vãng lai",
            PhoneNumber = user?.PhoneNumber ?? "",
            ShippingAddress = user?.Address ?? "",
            Status = "Chờ xác nhận",
            CreatedAt = DateTime.Now,
            TotalAmount = cart.Sum(x => x.LineTotal),
            Items = cart.Select(x => new CustomerOrderItem
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Price = x.Price,
                Quantity = x.Quantity,
                LineTotal = x.LineTotal
            }).ToList()
        };

        _context.CustomerOrders.Add(order);
        await _context.SaveChangesAsync();

        HttpContext.Session.Remove(CartSessionKey);

        TempData["SuccessMessage"] = $"Đặt hàng thành công. Mã đơn hàng của bạn là {order.OrderCode}.";

        return RedirectToAction("Index", "Home");
    }

    private List<CartItemViewModel> GetCart()
    {
        return HttpContext.Session.GetJson<List<CartItemViewModel>>(CartSessionKey) ?? [];
    }

    private void SaveCart(List<CartItemViewModel> cart)
    {
        HttpContext.Session.SetJson(CartSessionKey, cart);
    }
}