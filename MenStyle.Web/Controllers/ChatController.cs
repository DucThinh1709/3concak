using MenStyle.Web.Data;
using MenStyle.Web.Models;
using MenStyle.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MenStyle.Web.Controllers;

public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        var message = request.Message?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(message))
        {
            return Json(new ChatResponse
            {
                Reply = "Bạn hãy nhập câu hỏi để MENBOT hỗ trợ nhé.",
                QuickReplies = DefaultQuickReplies()
            });
        }

        if (message.Length > 300)
        {
            return Json(new ChatResponse
            {
                Reply = "Câu hỏi hơi dài rồi. Bạn hãy nhập ngắn gọn hơn để mình hỗ trợ tốt hơn nhé.",
                QuickReplies = DefaultQuickReplies()
            });
        }

        var normalized = NormalizeText(message);

        if (ContainsAny(normalized, "xin chao", "chao", "hello", "hi", "hey"))
        {
            return Json(new ChatResponse
            {
                Reply = "Xin chào, mình là MENBOT. Mình có thể hỗ trợ bạn tìm sản phẩm, xem sale, kiểm tra giỏ hàng, xem đơn hàng, hướng dẫn thanh toán và giao hàng.",
                QuickReplies = DefaultQuickReplies()
            });
        }

        if (ContainsAny(normalized, "gio hang", "cart", "trong gio co gi", "kiem tra gio"))
        {
            return Json(await BuildCartReplyAsync());
        }

        if (ContainsAny(normalized, "don hang", "trang thai don", "theo doi don", "ma don", "order"))
        {
            return Json(await BuildOrderReplyAsync(message));
        }

        if (ContainsAny(normalized, "ship", "giao hang", "van chuyen", "phi ship", "mien phi van chuyen"))
        {
            return Json(new ChatResponse
            {
                Reply = "MENSTYLE hỗ trợ giao hàng nội thành. Đơn từ 499.000đ được miễn phí vận chuyển. Khi thanh toán, bạn có thể xác nhận vị trí giao hàng bằng bản đồ.",
                ActionText = "Đi tới giỏ hàng",
                ActionUrl = "/Cart",
                QuickReplies =
                [
                    new ChatQuickReply { Text = "Thanh toán QR", Question = "Thanh toán bằng QR như thế nào?" },
                    new ChatQuickReply { Text = "Xem giỏ hàng", Question = "Kiểm tra giỏ hàng của tôi" },
                    new ChatQuickReply { Text = "Tìm áo thun", Question = "Tìm áo thun dưới 300k" }
                ]
            });
        }

        if (ContainsAny(normalized, "thanh toan", "chuyen khoan", "cod", "qr", "vietqr", "ngan hang"))
        {
            return Json(new ChatResponse
            {
                Reply = "Website hỗ trợ 2 phương thức: thanh toán khi nhận hàng và chuyển khoản bằng mã QR. Nếu chọn chuyển khoản, hệ thống sẽ tạo QR theo mã đơn, số tiền và nội dung chuyển khoản tự động.",
                ActionText = "Mở giỏ hàng",
                ActionUrl = "/Cart",
                QuickReplies =
                [
                    new ChatQuickReply { Text = "Đơn hàng", Question = "Xem đơn hàng của tôi" },
                    new ChatQuickReply { Text = "Sale", Question = "Sản phẩm đang sale" },
                    new ChatQuickReply { Text = "Giao hàng", Question = "Shop có giao hàng không?" }
                ]
            });
        }

        if (ContainsAny(normalized, "size", "co ao", "kich thuoc", "chon size"))
        {
            return Json(new ChatResponse
            {
                Reply = "Ở trang chi tiết sản phẩm, bạn có thể chọn size S, M, L, XL tùy từng sản phẩm. Size đã chọn sẽ được lưu vào giỏ hàng và lưu tiếp vào đơn hàng sau khi đặt.",
                ActionText = "Xem sản phẩm",
                ActionUrl = "/Home/SanPham",
                QuickReplies =
                [
                    new ChatQuickReply { Text = "Tìm áo thun", Question = "Tìm áo thun" },
                    new ChatQuickReply { Text = "Tìm áo khoác", Question = "Tìm áo khoác" },
                    new ChatQuickReply { Text = "Màu sắc", Question = "Màu sản phẩm hoạt động như thế nào?" }
                ]
            });
        }

        if (ContainsAny(normalized, "mau", "mau sac", "color", "anh theo mau"))
        {
            return Json(new ChatResponse
            {
                Reply = "Ở trang chi tiết sản phẩm, khi bạn chọn màu, ảnh sản phẩm sẽ đổi theo màu nếu admin đã nhập ảnh theo màu. Màu, size và ảnh đã chọn đều được lưu vào giỏ hàng.",
                ActionText = "Xem sản phẩm",
                ActionUrl = "/Home/SanPham",
                QuickReplies = DefaultQuickReplies()
            });
        }

        if (ContainsAny(normalized, "doi tra", "tra hang", "hoan hang", "bao hanh"))
        {
            return Json(new ChatResponse
            {
                Reply = "Chính sách đổi trả demo: khách hàng nên kiểm tra sản phẩm khi nhận hàng. Nếu sản phẩm lỗi, sai mẫu, sai size hoặc sai màu so với đơn, shop sẽ hỗ trợ đổi trả theo quy định.",
                QuickReplies =
                [
                    new ChatQuickReply { Text = "Đơn hàng", Question = "Xem đơn hàng của tôi" },
                    new ChatQuickReply { Text = "Giao hàng", Question = "Shop có giao hàng không?" },
                    new ChatQuickReply { Text = "Thanh toán", Question = "Thanh toán bằng QR được không?" }
                ]
            });
        }

        var productReply = await BuildProductSearchReplyAsync(message, normalized);

        if (!string.IsNullOrWhiteSpace(productReply.Reply) || productReply.Products.Any())
        {
            return Json(productReply);
        }

        return Json(new ChatResponse
        {
            Reply = "Mình chưa hiểu rõ ý bạn. Bạn có thể hỏi theo kiểu tự nhiên hơn như: “Có áo thun nào rẻ không?”, “Cho mình xem hàng đang sale”, “Tìm áo khoác”, “Giỏ hàng của tôi”, hoặc “Đơn hàng mới nhất của tôi”.",
            QuickReplies = DefaultQuickReplies()
        });
    }

    private async Task<ChatResponse> BuildCartReplyAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return new ChatResponse
            {
                Reply = "Bạn cần đăng nhập để MENBOT kiểm tra giỏ hàng.",
                ActionText = "Đăng nhập",
                ActionUrl = "/Account/Login",
                QuickReplies =
                [
                    new ChatQuickReply { Text = "Tìm sản phẩm", Question = "Tìm áo thun" },
                    new ChatQuickReply { Text = "Sale", Question = "Sản phẩm đang sale" }
                ]
            };
        }

        var cartItems = await _context.ShoppingCartItems
            .Include(x => x.Product)
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();

        if (!cartItems.Any())
        {
            return new ChatResponse
            {
                Reply = "Giỏ hàng của bạn hiện đang trống. Bạn có thể xem sản phẩm hoặc xem các món đang sale.",
                ActionText = "Xem sản phẩm",
                ActionUrl = "/Home/SanPham",
                QuickReplies =
                [
                    new ChatQuickReply { Text = "Sale", Question = "Sản phẩm đang sale" },
                    new ChatQuickReply { Text = "Áo thun", Question = "Tìm áo thun" }
                ]
            };
        }

        var totalQuantity = cartItems.Sum(x => x.Quantity);
        var total = cartItems.Sum(x => (x.Product?.Price ?? 0) * x.Quantity);

        var lines = cartItems
            .Take(4)
            .Select(x =>
            {
                var productName = x.Product?.Name ?? "Sản phẩm";
                var variant = "";

                if (!string.IsNullOrWhiteSpace(x.SelectedSize))
                {
                    variant += $" | Size {x.SelectedSize}";
                }

                if (!string.IsNullOrWhiteSpace(x.SelectedColor))
                {
                    variant += $" | Màu {x.SelectedColor}";
                }

                return $"- {productName}{variant} | SL: {x.Quantity}";
            });

        return new ChatResponse
        {
            Reply = $"Giỏ hàng của bạn đang có {totalQuantity} sản phẩm.\n{string.Join("\n", lines)}\nTạm tính: {FormatPrice(total)}",
            ActionText = "Mở giỏ hàng",
            ActionUrl = "/Cart",
            QuickReplies =
            [
                new ChatQuickReply { Text = "Thanh toán", Question = "Tôi muốn thanh toán" },
                new ChatQuickReply { Text = "Xem đơn hàng", Question = "Xem đơn hàng của tôi" },
                new ChatQuickReply { Text = "Sale", Question = "Sản phẩm đang sale" }
            ]
        };
    }

    private async Task<ChatResponse> BuildOrderReplyAsync(string rawMessage)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return new ChatResponse
            {
                Reply = "Bạn cần đăng nhập để MENBOT kiểm tra đơn hàng.",
                ActionText = "Đăng nhập",
                ActionUrl = "/Account/Login"
            };
        }

        var orderCode = ExtractOrderCode(rawMessage);

        CustomerOrder? order;

        if (!string.IsNullOrWhiteSpace(orderCode))
        {
            order = await _context.CustomerOrders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }
        else
        {
            order = await _context.CustomerOrders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        if (order == null)
        {
            return new ChatResponse
            {
                Reply = string.IsNullOrWhiteSpace(orderCode)
                    ? "Mình chưa tìm thấy đơn hàng nào trong tài khoản của bạn."
                    : $"Mình không tìm thấy đơn hàng {orderCode} trong tài khoản của bạn.",
                ActionText = "Xem sản phẩm",
                ActionUrl = "/Home/SanPham",
                QuickReplies = DefaultQuickReplies()
            };
        }

        var itemText = order.Items.Any()
            ? string.Join("\n", order.Items.Take(3).Select(i => $"- {i.ProductName} | SL: {i.Quantity} | {FormatPrice(i.LineTotal)}"))
            : "- Chưa có chi tiết sản phẩm";

        return new ChatResponse
        {
            Reply =
                $"Đơn gần nhất của bạn:\n" +
                $"Mã đơn: {order.OrderCode}\n" +
                $"Ngày đặt: {order.CreatedAt:dd/MM/yyyy HH:mm}\n" +
                $"Trạng thái: {order.Status}\n" +
                $"Thanh toán: {order.PaymentMethod} - {order.PaymentStatus}\n" +
                $"Tổng tiền: {FormatPrice(order.TotalAmount)}\n" +
                $"Sản phẩm:\n{itemText}",
            ActionText = "Xem chi tiết đơn",
            ActionUrl = $"/Account/MyOrderDetails/{order.Id}",
            QuickReplies =
            [
                new ChatQuickReply { Text = "Giỏ hàng", Question = "Kiểm tra giỏ hàng của tôi" },
                new ChatQuickReply { Text = "Mua thêm", Question = "Tìm áo thun" },
                new ChatQuickReply { Text = "Đổi trả", Question = "Chính sách đổi trả như thế nào?" }
            ]
        };
    }

    private async Task<ChatResponse> BuildProductSearchReplyAsync(string rawMessage, string normalizedMessage)
    {
        var wantsSale = ContainsAny(
            normalizedMessage,
            "sale",
            "dang sale",
            "giam gia",
            "khuyen mai",
            "uu dai",
            "ha gia",
            "sale off"
        );

        var wantsNewest = ContainsAny(
            normalizedMessage,
            "hang moi",
            "moi nhat",
            "san pham moi",
            "new",
            "vua ve"
        );

        var wantsCheap = ContainsAny(
            normalizedMessage,
            "re",
            "gia re",
            "duoi",
            "nho hon",
            "thap hon",
            "tam gia"
        );

        var wantsExpensive = ContainsAny(
            normalizedMessage,
            "tren",
            "cao hon",
            "dat",
            "premium",
            "cao cap"
        );

        var categorySlug = DetectCategorySlug(normalizedMessage);
        var priceRange = ExtractPriceRange(normalizedMessage);

        var isProductIntent =
            wantsSale ||
            wantsNewest ||
            wantsCheap ||
            wantsExpensive ||
            !string.IsNullOrWhiteSpace(categorySlug) ||
            priceRange.MinPrice != null ||
            priceRange.MaxPrice != null ||
            ContainsAny(
                normalizedMessage,
                "tim",
                "kiem",
                "san pham",
                "hang",
                "mua",
                "ao",
                "quan",
                "do nam",
                "shop co",
                "cho xem"
            );

        if (!isProductIntent)
        {
            return new ChatResponse();
        }

        var allProducts = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        var filtered = allProducts.AsEnumerable();

        if (wantsSale)
        {
            filtered = filtered.Where(p => p.OldPrice > p.Price && p.OldPrice > 0);
        }

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            filtered = filtered.Where(p => p.CategorySlug == categorySlug);
        }

        if (priceRange.MinPrice != null)
        {
            filtered = filtered.Where(p => p.Price >= priceRange.MinPrice.Value);
        }

        if (priceRange.MaxPrice != null)
        {
            filtered = filtered.Where(p => p.Price <= priceRange.MaxPrice.Value);
        }

        var keywordTokens = ExtractSearchTokens(normalizedMessage);

        if (keywordTokens.Any())
        {
            filtered = filtered.Where(p =>
            {
                var productText = NormalizeText(
                    $"{p.Name} {p.CategoryName} {p.CategorySlug} {p.Description} {p.Material} {p.Fit}"
                );

                return keywordTokens.Any(token => productText.Contains(token));
            });
        }

        if (wantsNewest)
        {
            filtered = filtered.OrderByDescending(p => p.Id);
        }
        else if (wantsSale)
        {
            filtered = filtered.OrderByDescending(p => GetSalePercent(p.OldPrice, p.Price));
        }
        else if (wantsCheap || priceRange.MaxPrice != null)
        {
            filtered = filtered.OrderBy(p => p.Price);
        }
        else if (wantsExpensive || priceRange.MinPrice != null)
        {
            filtered = filtered.OrderByDescending(p => p.Price);
        }
        else
        {
            filtered = filtered.OrderByDescending(p => p.Id);
        }

        var products = filtered
            .Take(4)
            .ToList();

        if (!products.Any())
        {
            if (wantsSale)
            {
                return new ChatResponse
                {
                    Reply = "Hiện tại mình chưa tìm thấy sản phẩm nào đang sale. Bạn có thể xem toàn bộ sản phẩm hoặc thử tìm theo danh mục như áo thun, áo khoác, quần nam.",
                    ActionText = "Xem tất cả sản phẩm",
                    ActionUrl = "/Home/SanPham",
                    QuickReplies =
                    [
                        new ChatQuickReply { Text = "Áo thun", Question = "Tìm áo thun" },
                    new ChatQuickReply { Text = "Áo khoác", Question = "Tìm áo khoác" },
                    new ChatQuickReply { Text = "Quần nam", Question = "Tìm quần nam" }
                    ]
                };
            }

            return new ChatResponse
            {
                Reply = "Mình chưa tìm thấy sản phẩm phù hợp với yêu cầu này. Bạn có thể thử nhập đơn giản hơn, ví dụ: “áo thun”, “áo khoác”, “quần nam”, “sản phẩm dưới 300k”.",
                ActionText = "Xem sản phẩm",
                ActionUrl = "/Home/SanPham",
                QuickReplies = DefaultQuickReplies()
            };
        }

        return new ChatResponse
        {
            Reply = BuildProductReplyIntro(products.Count, wantsSale, categorySlug, priceRange),
            Products = products.Select(ToChatProductSuggestion).ToList(),
            ActionText = wantsSale ? "Xem tất cả hàng đang sale" : "Xem tất cả sản phẩm",
            ActionUrl = wantsSale ? "/Home/SanPham?saleOnly=true" : "/Home/SanPham",
            QuickReplies =
            [
                new ChatQuickReply { Text = "Sale", Question = "Sản phẩm đang sale" },
            new ChatQuickReply { Text = "Dưới 300k", Question = "Tìm sản phẩm dưới 300k" },
            new ChatQuickReply { Text = "Áo thun", Question = "Tìm áo thun" },
            new ChatQuickReply { Text = "Áo khoác", Question = "Tìm áo khoác" }
            ]
        };
    }

    private static ChatProductSuggestion ToChatProductSuggestion(Product product)
    {
        var salePercent = GetSalePercent(product.OldPrice, product.Price);

        return new ChatProductSuggestion
        {
            Id = product.Id,
            Name = product.Name,
            CategoryName = product.CategoryName,
            ImageUrl = product.ImageUrl,
            Price = FormatPrice(product.Price),
            OldPrice = salePercent > 0 ? FormatPrice(product.OldPrice) : "",
            SalePercent = salePercent,
            Url = $"/Home/ChiTietSanPham/{product.Id}"
        };
    }

    private static string BuildProductReplyIntro(
        int count,
        bool wantsSale,
        string categorySlug,
        (decimal? MinPrice, decimal? MaxPrice) priceRange)
    {
        if (wantsSale)
        {
            return $"Mình tìm thấy {count} sản phẩm đang sale phù hợp:";
        }

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            return $"Mình tìm thấy {count} sản phẩm phù hợp với danh mục bạn hỏi:";
        }

        if (priceRange.MinPrice != null || priceRange.MaxPrice != null)
        {
            return $"Mình tìm thấy {count} sản phẩm phù hợp với mức giá bạn muốn:";
        }

        return $"Mình tìm thấy {count} sản phẩm phù hợp:";
    }

    private static string DetectCategorySlug(string normalizedMessage)
    {
        if (ContainsAny(normalizedMessage, "ao thun", "t shirt", "tshirt", "polo"))
        {
            return "ao-thun";
        }

        if (ContainsAny(normalizedMessage, "so mi", "ao so mi", "somi"))
        {
            return "so-mi";
        }

        if (ContainsAny(normalizedMessage, "quan", "jean", "kaki", "short", "jogger", "quan nam"))
        {
            return "quan";
        }

        if (ContainsAny(normalizedMessage, "ao khoac", "jacket", "bomber", "hoodie"))
        {
            return "ao-khoac";
        }

        return string.Empty;
    }

    private static List<string> ExtractSearchTokens(string normalizedMessage)
    {
        var stopWords = new HashSet<string>
    {
        "tim", "kiem", "minh", "toi", "em", "anh", "chi", "ban",
        "muon", "can", "cho", "shop", "co", "khong", "xem",
        "san", "pham", "hang", "muc", "loai",
        "gia", "duoi", "tren", "tu", "den", "tam",
        "sale", "dang", "giam", "khuyen", "mai", "uu", "dai", "ha",
        "re", "dat", "cao", "thap", "hon", "nhat",
        "mau", "size", "ao", "quan", "nam"
    };

        var words = normalizedMessage
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(w => w.Length >= 3 && !stopWords.Contains(w))
            .Distinct()
            .ToList();

        return words;
    }

    private static (decimal? MinPrice, decimal? MaxPrice) ExtractPriceRange(string normalizedMessage)
    {
        var numbers = Regex.Matches(normalizedMessage, @"\d+")
            .Select(m => decimal.Parse(m.Value))
            .ToList();

        if (!numbers.Any())
        {
            return (null, null);
        }

        decimal NormalizeMoney(decimal value)
        {
            if (value < 1000)
            {
                return value * 1000;
            }

            return value;
        }

        if (ContainsAny(normalizedMessage, "duoi", "nho hon", "thap hon", "toi da", "max"))
        {
            return (null, NormalizeMoney(numbers.First()));
        }

        if (ContainsAny(normalizedMessage, "tren", "lon hon", "cao hon", "toi thieu", "min"))
        {
            return (NormalizeMoney(numbers.First()), null);
        }

        if (ContainsAny(normalizedMessage, "tu") && ContainsAny(normalizedMessage, "den") && numbers.Count >= 2)
        {
            var first = NormalizeMoney(numbers[0]);
            var second = NormalizeMoney(numbers[1]);

            return (Math.Min(first, second), Math.Max(first, second));
        }

        return (null, null);
    }

    private static string ExtractOrderCode(string rawMessage)
    {
        var match = Regex.Match(rawMessage.ToUpperInvariant(), @"MS\d{12,20}");

        return match.Success ? match.Value : string.Empty;
    }

    private static List<ChatQuickReply> DefaultQuickReplies()
    {
        return
        [
            new ChatQuickReply
        {
            Text = "Hàng đang sale",
            Question = "Cho mình xem sản phẩm đang sale"
        },
        new ChatQuickReply
        {
            Text = "Áo thun dưới 300k",
            Question = "Có áo thun nào dưới 300k không?"
        },
        new ChatQuickReply
        {
            Text = "Áo khoác",
            Question = "Cho mình xem áo khoác"
        },
        new ChatQuickReply
        {
            Text = "Giỏ hàng",
            Question = "Kiểm tra giỏ hàng của tôi"
        },
        new ChatQuickReply
        {
            Text = "Đơn hàng",
            Question = "Xem đơn hàng mới nhất của tôi"
        }
        ];
    }

    private static bool ContainsAny(string source, params string[] keywords)
    {
        return keywords.Any(source.Contains);
    }

    private static int GetSalePercent(decimal oldPrice, decimal price)
    {
        if (oldPrice <= 0 || oldPrice <= price)
        {
            return 0;
        }

        return (int)Math.Round((double)((oldPrice - price) / oldPrice * 100));
    }

    private static string FormatPrice(decimal price)
    {
        return $"{price:N0}đ".Replace(",", ".");
    }

    private static string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);

            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace("đ", "d")
            .Replace("₫", "d")
            .Replace(",", " ")
            .Replace(".", " ")
            .Replace("-", " ");
    }
}