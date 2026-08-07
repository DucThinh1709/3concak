using MenStyle.Web.Data;
using MenStyle.Web.Models;
using MenStyle.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MenStyle.Web.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CartController> _logger;

    public CartController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<CartController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
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
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            var safeReturnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("SanPham", "Home");

            return RedirectToAction("Login", "Account", new { returnUrl = safeReturnUrl });
        }

        var result = await AddToCartAsync(
            user.Id,
            id,
            selectedSize,
            selectedColor,
            quantity,
            cancellationToken);

        if (result.NotFound)
        {
            return NotFound();
        }

        if (!result.Succeeded)
        {
            return RedirectAfterAddError(result.Message, id, returnUrl);
        }

        TempData["SuccessMessage"] = result.Message;

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuick(
        int id,
        string? selectedSize = null,
        string? selectedColor = null,
        int quantity = 1,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ.",
                loginUrl = Url.Action("Login", "Account", new
                {
                    returnUrl = Url.Action("SanPham", "Home")
                })
            });
        }

        var result = await AddToCartAsync(
            user.Id,
            id,
            selectedSize,
            selectedColor,
            quantity,
            cancellationToken);

        if (result.NotFound)
        {
            return NotFound(new
            {
                success = false,
                message = result.Message
            });
        }

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                success = false,
                message = result.Message,
                cartQuantity = result.CartQuantity
            });
        }

        return Json(new
        {
            success = true,
            message = result.Message,
            cartQuantity = result.CartQuantity
        });
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> MiniCart(
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var items = await GetCartAsync(user.Id, cancellationToken);

        return PartialView("_MiniCart", new CartViewModel
        {
            Items = items
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMiniCartItem(
        int id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Phiên đăng nhập đã hết hạn."
            });
        }

        var item = await _context.ShoppingCartItems
            .FirstOrDefaultAsync(
                x => x.UserId == user.Id && x.Id == id,
                cancellationToken);

        if (item == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Sản phẩm không còn trong giỏ hàng."
            });
        }

        _context.ShoppingCartItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        var cartQuantity = await _context.ShoppingCartItems
            .Where(x => x.UserId == user.Id)
            .SumAsync(x => (int?)x.Quantity, cancellationToken) ?? 0;

        return Json(new
        {
            success = true,
            message = "Đã xóa sản phẩm khỏi giỏ hàng.",
            cartQuantity
        });
    }

    private async Task<CartMutationResult> AddToCartAsync(
        string userId,
        int productId,
        string? selectedSize,
        string? selectedColor,
        int quantity,
        CancellationToken cancellationToken)
    {
        var cartQuantity = 0;

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var userCartItems = await _context.ShoppingCartItems
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.ProductId)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            cartQuantity = userCartItems.Sum(x => x.Quantity);

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.Id == productId && p.IsActive,
                    cancellationToken);

            if (product == null)
            {
                return new CartMutationResult(
                    false,
                    "Sản phẩm không tồn tại hoặc đã ngừng bán.",
                    cartQuantity,
                    NotFound: true);
            }

            if (product.StockQuantity <= 0)
            {
                return new CartMutationResult(
                    false,
                    "Sản phẩm đã hết hàng và không thể thêm vào giỏ.",
                    cartQuantity);
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

            if (sizes.Any() && !sizes.Contains(finalSize))
            {
                return new CartMutationResult(
                    false,
                    "Size đã chọn không hợp lệ.",
                    cartQuantity);
            }

            if (colors.Any() && !colors.Contains(finalColor))
            {
                return new CartMutationResult(
                    false,
                    "Màu sắc đã chọn không hợp lệ.",
                    cartQuantity);
            }

            quantity = Math.Max(1, quantity);

            var currentProductQuantity = userCartItems
                .Where(x => x.ProductId == product.Id)
                .Sum(x => x.Quantity);

            var remainingQuantity = product.StockQuantity - currentProductQuantity;

            if (remainingQuantity <= 0)
            {
                return new CartMutationResult(
                    false,
                    $"Bạn đã có đủ {product.StockQuantity} sản phẩm này trong giỏ, bằng với tồn kho hiện tại.",
                    cartQuantity);
            }

            if (quantity > remainingQuantity)
            {
                return new CartMutationResult(
                    false,
                    $"Không thể thêm {quantity} sản phẩm. Bạn chỉ có thể thêm tối đa {remainingQuantity} sản phẩm nữa.",
                    cartQuantity);
            }

            var existingItem = userCartItems.FirstOrDefault(x =>
                x.ProductId == product.Id
                && x.SelectedSize == finalSize
                && x.SelectedColor == finalColor);

            var selectedImageUrl = GetSelectedImageUrl(product, finalColor);

            if (existingItem == null)
            {
                _context.ShoppingCartItems.Add(new ShoppingCartItem
                {
                    UserId = userId,
                    ProductId = product.Id,
                    SelectedSize = finalSize,
                    SelectedColor = finalColor,
                    SelectedImageUrl = selectedImageUrl,
                    Quantity = quantity,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                existingItem.Quantity += quantity;
                existingItem.SelectedImageUrl = selectedImageUrl;
                existingItem.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new CartMutationResult(
                true,
                "Đã thêm sản phẩm vào giỏ hàng.",
                cartQuantity + quantity);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Không thể thêm product {ProductId} vào giỏ của user {UserId}.",
                productId,
                userId);

            return new CartMutationResult(
                false,
                "Chưa thể cập nhật giỏ hàng. Vui lòng thử lại.",
                cartQuantity);
        }
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

        var colorCount = random.Next(1, 4);
        // random từ 1 đến 3 màu
        // Next(1, 4) nghĩa là lấy 1, 2 hoặc 3

        return colorPool
            .OrderBy(_ => random.Next())
            .Take(colorCount)
            .ToList();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Increase(
        int id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var userCartItems = await _context.ShoppingCartItems
                .Where(x => x.UserId == user.Id)
                .OrderBy(x => x.ProductId)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            var item = userCartItems.FirstOrDefault(x => x.Id == id);

            if (item == null)
            {
                return RedirectToAction("Index");
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == item.ProductId, cancellationToken);

            if (product == null || !product.IsActive)
            {
                TempData["ErrorMessage"] = "Sản phẩm không còn được bán nên không thể tăng số lượng.";
                return RedirectToAction("Index");
            }

            var currentProductQuantity = userCartItems
                .Where(x => x.ProductId == item.ProductId)
                .Sum(x => x.Quantity);

            if (product.StockQuantity <= 0)
            {
                TempData["ErrorMessage"] = $"{product.Name} đã hết hàng.";
                return RedirectToAction("Index");
            }

            if (currentProductQuantity >= product.StockQuantity)
            {
                TempData["ErrorMessage"] =
                    $"Không thể tăng thêm. {product.Name} chỉ còn {product.StockQuantity} sản phẩm.";

                return RedirectToAction("Index");
            }

            item.Quantity++;
            item.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return RedirectToAction("Index");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Không thể tăng cart item {CartItemId} của user {UserId}.",
                id,
                user.Id);

            TempData["ErrorMessage"] = "Chưa thể cập nhật giỏ hàng. Vui lòng thử lại.";
            return RedirectToAction("Index");
        }
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

        var stockError = GetCartStockError(cart);

        if (stockError != null)
        {
            TempData["ErrorMessage"] = stockError;
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
    public async Task<IActionResult> Checkout(
        CheckoutViewModel model,
        CancellationToken cancellationToken)
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

        var stockError = GetCartStockError(cart);

        if (stockError != null)
        {
            TempData["ErrorMessage"] = stockError;
            return RedirectToAction("Index");
        }

        if (model.NoNote)
        {
            ModelState.Remove(nameof(model.Note));
            model.Note = "";
        }

        if (model.ShippingLatitude == null || model.ShippingLongitude == null)
        {
            ModelState.AddModelError(nameof(model.ShippingAddress),
                "Vui lòng chọn vị trí giao hàng trên bản đồ.");
        }

        if (!model.IsAddressConfirmed)
        {
            ModelState.AddModelError(nameof(model.ShippingAddress),
                "Vui lòng bấm Xác nhận địa chỉ trước khi đặt hàng.");
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

        await using var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        try
        {
            var cartItems = await _context.ShoppingCartItems
                .Include(x => x.Product)
                .Where(x => x.UserId == user.Id)
                .OrderBy(x => x.ProductId)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            if (!cartItems.Any())
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["ErrorMessage"] = "Giỏ hàng đang trống, chưa thể đặt hàng.";
                return RedirectToAction("Index");
            }

            foreach (var productGroup in cartItems.GroupBy(x => x.ProductId))
            {
                var firstItem = productGroup.First();
                var product = firstItem.Product;
                var requestedQuantity = productGroup.Sum(x => x.Quantity);

                if (product == null || !product.IsActive || requestedQuantity <= 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    TempData["ErrorMessage"] =
                        "Một sản phẩm trong giỏ không còn hợp lệ. Vui lòng kiểm tra lại giỏ hàng.";

                    return RedirectToAction("Index");
                }

                var affectedRows = await _context.Products
                    .Where(p => p.Id == product.Id
                                && p.IsActive
                                && p.StockQuantity >= requestedQuantity)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            p => p.StockQuantity,
                            p => p.StockQuantity - requestedQuantity),
                        cancellationToken);

                if (affectedRows != 1)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    TempData["ErrorMessage"] =
                        $"{product.Name} vừa thay đổi tồn kho và không còn đủ số lượng. Đơn hàng chưa được tạo.";

                    return RedirectToAction("Index");
                }
            }

            var orderItems = cartItems.Select(x =>
            {
                var product = x.Product!;
                var imageUrl = string.IsNullOrWhiteSpace(x.SelectedImageUrl)
                    ? product.ImageUrl
                    : x.SelectedImageUrl;

                return new CustomerOrderItem
                {
                    ProductId = x.ProductId,
                    ProductName = product.Name,
                    SelectedSize = x.SelectedSize,
                    SelectedColor = x.SelectedColor,
                    SelectedImageUrl = imageUrl,
                    Price = product.Price,
                    Quantity = x.Quantity,
                    LineTotal = product.Price * x.Quantity
                };
            }).ToList();

            var order = new CustomerOrder
            {
                OrderCode = GenerateOrderCode(),
                UserId = user.Id,
                CustomerName = model.CustomerName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                ShippingAddress = model.ShippingAddress.Trim(),
                ShippingLatitude = model.ShippingLatitude,
                ShippingLongitude = model.ShippingLongitude,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = paymentStatus,
                Note = model.NoNote ? "" : model.Note?.Trim() ?? "",
                Status = "Chờ xác nhận",
                CreatedAt = DateTime.Now,
                TotalAmount = orderItems.Sum(x => x.LineTotal),
                Items = orderItems
            };

            _context.CustomerOrders.Add(order);
            _context.ShoppingCartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return RedirectToAction("OrderSuccess", new { id = order.Id });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);

            _logger.LogError(
                exception,
                "Đặt hàng thất bại và đã hoàn tác tồn kho cho user {UserId}.",
                user.Id);

            TempData["ErrorMessage"] =
                "Chưa thể tạo đơn hàng. Tồn kho và giỏ hàng không bị thay đổi, vui lòng thử lại.";

            return RedirectToAction("Index");
        }
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

    private async Task<List<CartItemViewModel>> GetCartAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cartItems = await _context.ShoppingCartItems
            .AsNoTracking()
            .Include(x => x.Product)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var productTotals = cartItems
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(x => x.Quantity));

        return cartItems
            .Where(x => x.Product != null)
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
                Quantity = x.Quantity,
                StockQuantity = x.Product.StockQuantity,
                TotalProductQuantityInCart = productTotals[x.ProductId],
                IsProductActive = x.Product.IsActive
            })
            .ToList();
    }

    private IActionResult RedirectAfterAddError(
        string message,
        int productId,
        string? returnUrl)
    {
        TempData["ErrorMessage"] = message;

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("ChiTietSanPham", "Home", new { id = productId });
    }

    private static string? GetCartStockError(IEnumerable<CartItemViewModel> cart)
    {
        foreach (var productGroup in cart.GroupBy(x => x.ProductId))
        {
            var firstItem = productGroup.First();
            var requestedQuantity = productGroup.Sum(x => x.Quantity);

            if (!firstItem.IsProductActive)
            {
                return $"{firstItem.ProductName} không còn được bán. Vui lòng xóa sản phẩm khỏi giỏ.";
            }

            if (firstItem.StockQuantity <= 0)
            {
                return $"{firstItem.ProductName} đã hết hàng. Vui lòng xóa sản phẩm khỏi giỏ.";
            }

            if (requestedQuantity > firstItem.StockQuantity)
            {
                return $"{firstItem.ProductName} chỉ còn {firstItem.StockQuantity} sản phẩm, nhưng giỏ đang có {requestedQuantity}. Vui lòng giảm số lượng.";
            }
        }

        return null;
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
        return "MS" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
    }

    private sealed record CartMutationResult(
        bool Succeeded,
        string Message,
        int CartQuantity = 0,
        bool NotFound = false);
}
