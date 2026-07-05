using System;
using System.ComponentModel.DataAnnotations;

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
    public decimal Price { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá cũ phải lớn hơn hoặc bằng 0")]
    public decimal OldPrice { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập đường dẫn hình ảnh")]
    [StringLength(500, ErrorMessage = "Đường dẫn hình ảnh không được vượt quá 500 ký tự")]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Mô tả ảnh không được vượt quá 250 ký tự")]
    public string AltText { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}