using MenStyle.Web.Data;
using MenStyle.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenStyle.Web.Controllers;

public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        var message = request.Message?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(message))
        {
            return Json(new ChatResponse
            {
                Reply = "Bạn hãy nhập câu hỏi để MENBOT hỗ trợ nhé."
            });
        }

        if (message.Length > 300)
        {
            return Json(new ChatResponse
            {
                Reply = "Câu hỏi hơi dài rồi. Bạn hãy nhập ngắn gọn hơn để mình hỗ trợ tốt hơn nhé."
            });
        }

        var lower = message.ToLower();

        if (ContainsAny(lower, "xin chào", "chào", "hello", "hi"))
        {
            return Json(new ChatResponse
            {
                Reply = "Xin chào, mình là MENBOT. Mình có thể hỗ trợ bạn tìm sản phẩm, xem cách thanh toán, giao hàng và hướng dẫn đặt hàng."
            });
        }

        if (ContainsAny(lower, "ship", "giao hàng", "vận chuyển", "phí ship"))
        {
            return Json(new ChatResponse
            {
                Reply = "MENSTYLE hỗ trợ giao hàng nội thành. Với đơn từ 499.000đ, shop miễn phí vận chuyển. Khi thanh toán, phí vận chuyển sẽ hiển thị trong phần tóm tắt đơn hàng."
            });
        }

        if (ContainsAny(lower, "thanh toán", "chuyển khoản", "cod", "qr"))
        {
            return Json(new ChatResponse
            {
                Reply = "Website hiện hỗ trợ 2 phương thức: thanh toán khi nhận hàng và chuyển khoản ngân hàng bằng mã QR. Khi bạn đặt hàng, hệ thống sẽ tạo mã đơn và nội dung chuyển khoản tự động."
            });
        }

        if (ContainsAny(lower, "size", "cỡ", "kích thước"))
        {
            return Json(new ChatResponse
            {
                Reply = "Ở trang chi tiết sản phẩm, bạn có thể chọn size như S, M, L, XL tùy từng sản phẩm. Size đã chọn sẽ được lưu vào giỏ hàng và đơn hàng."
            });
        }

        if (ContainsAny(lower, "màu", "màu sắc", "color"))
        {
            return Json(new ChatResponse
            {
                Reply = "Ở trang chi tiết sản phẩm, bạn có thể chọn màu sản phẩm. Nếu sản phẩm có ảnh riêng theo màu, ảnh sẽ tự đổi đúng theo màu bạn chọn."
            });
        }

        if (ContainsAny(lower, "giỏ hàng", "cart"))
        {
            return Json(new ChatResponse
            {
                Reply = "Giỏ hàng của bạn được lưu theo tài khoản trong SQL Server. Vì vậy khi đăng nhập lại, sản phẩm trong giỏ vẫn còn nếu bạn chưa thanh toán."
            });
        }

        if (ContainsAny(lower, "đơn hàng", "trạng thái đơn", "theo dõi đơn"))
        {
            return Json(new ChatResponse
            {
                Reply = "Bạn có thể bấm nút Đơn hàng trên thanh menu để xem các đơn đã đặt, tổng tiền, trạng thái và chi tiết từng đơn."
            });
        }

        if (ContainsAny(lower, "đổi trả", "trả hàng", "hoàn hàng"))
        {
            return Json(new ChatResponse
            {
                Reply = "Chính sách đổi trả demo: khách hàng nên kiểm tra sản phẩm khi nhận hàng. Nếu sản phẩm lỗi hoặc sai mẫu, shop sẽ hỗ trợ đổi trả theo quy định của cửa hàng."
            });
        }

        var productReply = await SearchProductsAsync(message);

        if (!string.IsNullOrWhiteSpace(productReply))
        {
            return Json(new ChatResponse
            {
                Reply = productReply
            });
        }

        return Json(new ChatResponse
        {
            Reply = "Mình chưa hiểu rõ câu hỏi này. Bạn có thể hỏi theo mẫu: “Tìm áo thun”, “Shop có giao hàng không?”, “Thanh toán bằng QR được không?”, hoặc “Xem đơn hàng ở đâu?”."
        });
    }

    private async Task<string> SearchProductsAsync(string keyword)
    {
        var cleanKeyword = keyword.Trim();

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive &&
                        (p.Name.Contains(cleanKeyword)
                         || p.CategoryName.Contains(cleanKeyword)
                         || p.CategorySlug.Contains(cleanKeyword)))
            .OrderByDescending(p => p.Id)
            .Take(3)
            .ToListAsync();

        if (!products.Any())
        {
            return string.Empty;
        }

        var lines = new List<string>
        {
            $"Mình tìm thấy {products.Count} sản phẩm phù hợp:"
        };

        foreach (var product in products)
        {
            lines.Add($"- {product.Name} | {FormatPrice(product.Price)} | Xem: /Home/ChiTietSanPham/{product.Id}");
        }

        return string.Join("\n", lines);
    }

    private static bool ContainsAny(string source, params string[] keywords)
    {
        return keywords.Any(source.Contains);
    }

    private static string FormatPrice(decimal price)
    {
        return $"{price:N0}đ".Replace(",", ".");
    }
}