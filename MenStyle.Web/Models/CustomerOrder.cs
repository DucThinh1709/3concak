using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenStyle.Web.Models;

public class CustomerOrder
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string OrderCode { get; set; } = string.Empty;

    [StringLength(450)]
    public string? UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(255)]
    public string ShippingAddress { get; set; } = string.Empty;

    [StringLength(50)]
    public string PaymentMethod { get; set; } = "Thanh toán khi nhận hàng";

    [StringLength(50)]
    public string PaymentStatus { get; set; } = "Chưa thanh toán";

    [StringLength(500)]
    public string Note { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Chờ xác nhận";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<CustomerOrderItem> Items { get; set; } = [];
}