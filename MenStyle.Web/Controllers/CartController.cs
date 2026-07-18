using MenStyle.Web.Data;
using MenStyle.Web.Models;
using MenStyle.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenStyle.Web.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cart = await GetCartAsync(user.Id);

        var viewModel = new CartViewModel
        {
            Items = cart
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
    int id,
    string? selectedSize = null,
    string? selectedColor = null,
    int quantity = 1,
    string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            var safeReturnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("SanPham", "Home");

            return RedirectToAction("Login", "Account", new { returnUrl = safeReturnUrl });
        }

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
        {
            return NotFound();
        }

        var sizes = SplitOptions(product.AvailableSizes);
        var colors = SplitOptions(product.AvailableColors);

        if (!colors.Any())
        {
            colors = GenerateRandomColors(product.Id);
        }

        var finalSize = string.IsNullOrWhiteSpace(selectedSize)
            ? sizes.FirstOrDefault() ?? ""
            : selectedSize.Trim();

        var finalColor = string.IsNullOrWhiteSpace(selectedColor)
            ? colors.FirstOrDefault() ?? ""
            : selectedColor.Trim();

        var selectedImageUrl = GetSelectedImageUrl(product, finalColor);

        if (sizes.Any() && !sizes.Contains(finalSize))
        {
            ModelState.AddModelError(string.Empty, "Size không hợp lệ.");
            return RedirectToAction("ChiTietSanPham", "Home", new { id });
        }

        if (colors.Any() && !colors.Contains(finalColor))
        {
            ModelState.AddModelError(string.Empty, "Màu sắc không hợp lệ.");
            return RedirectToAction("ChiTietSanPham", "Home", new { id });
        }

        if (quantity < 1)
        {
            quantity = 1;
        }

        if (product.StockQuantity > 0 && quantity > product.StockQuantity)
        {
            quantity = product.StockQuantity;
        }

        var existingItem = await _context.ShoppingCartItems
            .FirstOrDefaultAsync(x =>
                x.UserId == user.Id
                && x.ProductId == product.Id
                && x.SelectedSize == finalSize
                && x.SelectedColor == finalColor);

        if (existingItem == null)
        {
            var cartItem = new ShoppingCartItem
            {
                UserId = user.Id,
                ProductId = product.Id,
                SelectedSize = finalSize,
                SelectedColor = finalColor,
                SelectedImageUrl = selectedImageUrl,
                Quantity = quantity,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.ShoppingCartItems.Add(cartItem);
        }
        else
        {
            existingItem.Quantity += quantity;
            existingItem.SelectedImageUrl = selectedImageUrl;
            existingItem.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index");
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
        "Xanh dương",
        "Đỏ",
        "Kem"
    };

        var random = new Random(productId);

        return colorPool
            .OrderBy(_ => random.Next())
            .Take(4)
            .ToList();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Increase(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var item = await _context.ShoppingCartItems
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Id == id);

        if (item != null)
        {
            item.Quantity++;
            item.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decrease(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var item = await _context.ShoppingCartItems
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Id == id);

        if (item != null)
        {
            item.Quantity--;

            if (item.Quantity <= 0)
            {
                _context.ShoppingCartItems.Remove(item);
            }
            else
            {
                item.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var item = await _context.ShoppingCartItems
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Id == id);

        if (item != null)
        {
            _context.ShoppingCartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var items = await _context.ShoppingCartItems
            .Where(x => x.UserId == user.Id)
            .ToListAsync();

        if (items.Any())
        {
            _context.ShoppingCartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng.";

        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cart = await GetCartAsync(user.Id);

        if (!cart.Any())
        {
            TempData["ErrorMessage"] = "Giỏ hàng đang trống, chưa thể thanh toán.";
            return RedirectToAction("Index");
        }

        var model = new CheckoutViewModel
        {
            Items = cart,
            OrderCode = GenerateOrderCode(),
            CustomerName = user.FullName ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            ShippingAddress = user.Address ?? "",
            PaymentMethod = "Thanh toán khi nhận hàng",
            NoNote = true,
            Note = ""
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cart = await GetCartAsync(user.Id);

        if (!cart.Any())
        {
            TempData["ErrorMessage"] = "Giỏ hàng đang trống, chưa thể đặt hàng.";
            return RedirectToAction("Index");
        }

        model.Items = cart;

        if (model.NoNote)
        {
            ModelState.Remove(nameof(model.Note));
            model.Note = "";
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var allowedPayments = new[]
        {
            "Thanh toán khi nhận hàng",
            "Chuyển khoản ngân hàng"
        };

        if (!allowedPayments.Contains(model.PaymentMethod))
        {
            ModelState.AddModelError(nameof(model.PaymentMethod), "Phương thức thanh toán không hợp lệ.");
            return View(model);
        }

        var paymentStatus = model.PaymentMethod == "Chuyển khoản ngân hàng"
            ? "Chờ thanh toán"
            : "Chưa thanh toán";

        var order = new CustomerOrder
        {
            OrderCode = string.IsNullOrWhiteSpace(model.OrderCode)
                ? GenerateOrderCode()
                : model.OrderCode,

            UserId = user.Id,
            CustomerName = model.CustomerName.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            ShippingAddress = model.ShippingAddress.Trim(),
            PaymentMethod = model.PaymentMethod,
            PaymentStatus = paymentStatus,
            Note = model.NoNote ? "" : model.Note?.Trim() ?? "",
            Status = "Chờ xác nhận",
            CreatedAt = DateTime.Now,
            TotalAmount = cart.Sum(x => x.LineTotal),

            Items = cart.Select(x => new CustomerOrderItem
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                SelectedSize = x.SelectedSize,
                SelectedColor = x.SelectedColor,
                SelectedImageUrl = x.ImageUrl,
                Price = x.Price,
                Quantity = x.Quantity,
                LineTotal = x.LineTotal
            }).ToList()
        };

        var cartItems = await _context.ShoppingCartItems
            .Where(x => x.UserId == user.Id)
            .ToListAsync();

        _context.CustomerOrders.Add(order);
        _context.ShoppingCartItems.RemoveRange(cartItems);

        await _context.SaveChangesAsync();

        return RedirectToAction("OrderSuccess", new { id = order.Id });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> OrderSuccess(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        var order = await _context.CustomerOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        if (!User.IsInRole("Admin") && order.UserId != user?.Id)
        {
            return Forbid();
        }

        return View(order);
    }

    private async Task<List<CartItemViewModel>> GetCartAsync(string userId)
    {
        return await _context.ShoppingCartItems
            .Include(x => x.Product)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new CartItemViewModel
            {
                CartItemId = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product!.Name,
                CategoryName = x.Product.CategoryName,
                ImageUrl = !string.IsNullOrWhiteSpace(x.SelectedImageUrl)
                    ? x.SelectedImageUrl
                    : x.Product.ImageUrl,
                SelectedSize = x.SelectedSize,
                SelectedColor = x.SelectedColor,
                Price = x.Product.Price,
                Quantity = x.Quantity
            })
            .ToListAsync();
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

    private static string GetSelectedImageUrl(Product product, string selectedColor)
    {
        var colorImages = ParseColorImageMap(product.ColorImageMap);

        if (!string.IsNullOrWhiteSpace(selectedColor)
            && colorImages.TryGetValue(selectedColor, out var imageUrl)
            && !string.IsNullOrWhiteSpace(imageUrl))
        {
            return imageUrl;
        }

        return product.ImageUrl;
    }

    private string GenerateOrderCode()
    {
        return "MS" + DateTime.Now.ToString("yyyyMMddHHmmss");
    }
}