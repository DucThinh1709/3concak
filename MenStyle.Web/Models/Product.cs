using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenStyle.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
    [StringLength(150, ErrorMessage = "Tên sản phẩm không được vượt quá 150 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã danh mục")]
    [StringLength(100, ErrorMessage = "Mã danh mục không được vượt quá 100 ký tự")]
    public string CategorySlug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
    [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá cũ phải lớn hơn hoặc bằng 0")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OldPrice { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập đường dẫn hình ảnh")]
    [StringLength(500, ErrorMessage = "Đường dẫn hình ảnh không được vượt quá 500 ký tự")]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(2000)]
    public string ColorImageMap { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Mô tả ảnh không được vượt quá 250 ký tự")]
    public string AltText { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = "Sản phẩm thời trang nam hiện đại, dễ phối đồ, phù hợp đi học, đi làm và đi chơi.";

    [StringLength(100)]
    public string Material { get; set; } = "Cotton pha Polyester";

    [StringLength(100)]
    public string Fit { get; set; } = "Regular fit";

    [StringLength(100)]
    public string AvailableSizes { get; set; } = "S,M,L,XL";

    [StringLength(150)]
    public string AvailableColors { get; set; } = string.Empty; 

    [StringLength(500)]
    public string CareInstruction { get; set; } = "Giặt máy ở chế độ nhẹ, không dùng chất tẩy mạnh, ủi ở nhiệt độ thấp.";

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; } = 20;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [StringLength(500)]
    public string? ProductTags { get; set; } = string.Empty;
}