using MenStyle.Web.Extensions;
using MenStyle.Web.Models;
using MenStyle.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MenStyle.Web.Controllers;

public class CartController : Controller
{
    private const string CartSessionKey = "MENSTYLE_CART";

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
    public IActionResult Add(int id, string? returnUrl = null)
    {
        var product = ProductCatalog.FindById(id);

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
    public IActionResult Checkout()
    {
        var cart = GetCart();

        if (!cart.Any())
        {
            TempData["ErrorMessage"] = "Giỏ hàng đang trống, chưa thể đặt hàng.";
            return RedirectToAction("Index");
        }

        HttpContext.Session.Remove(CartSessionKey);

        TempData["SuccessMessage"] = "Đặt hàng thành công. Cảm ơn bạn đã mua hàng tại MENSTYLE!";

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