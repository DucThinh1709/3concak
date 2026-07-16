using System.ComponentModel.DataAnnotations;

namespace MenStyle.Web.ViewModels;

public class CheckoutViewModel
{
    public List<CartItemViewModel> Items { get; set; } = [];

    public decimal Total => Items.Sum(x => x.LineTotal);

    public int TotalQuantity => Items.Sum(x => x.Quantity);

    public string OrderCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
    [StringLength(100)]
    [Display(Name = "Họ tên người nhận")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
    [StringLength(255)]
    [Display(Name = "Địa chỉ giao hàng")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
    [Display(Name = "Phương thức thanh toán")]
    public string PaymentMethod { get; set; } = "Thanh toán khi nhận hàng";

    [Display(Name = "Không có ghi chú")]
    public bool NoNote { get; set; } = true;

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }
}