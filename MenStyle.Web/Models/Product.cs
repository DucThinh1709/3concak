using System.ComponentModel.DataAnnotations;

namespace MenStyle.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nh?p tên s?n ph?m")]
    [StringLength(150, ErrorMessage = "Tên s?n ph?m không ???c v??t quá 150 ký t?")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nh?p mã danh m?c")]
    [StringLength(100, ErrorMessage = "Mã danh m?c không ???c v??t quá 100 ký t?")]
    public string CategorySlug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nh?p tên danh m?c")]
    [StringLength(100, ErrorMessage = "Tên danh m?c không ???c v??t quá 100 ký t?")]
    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nh?p giá s?n ph?m")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá s?n ph?m ph?i l?n h?n ho?c b?ng 0")]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá c? ph?i l?n h?n ho?c b?ng 0")]
    public decimal OldPrice { get; set; }

    [Required(ErrorMessage = "Vui lòng nh?p ???ng d?n hình ?nh")]
    [StringLength(500, ErrorMessage = "???ng d?n hình ?nh không ???c v??t quá 500 ký t?")]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Mô t? ?nh không ???c v??t quá 250 ký t?")]
    public string AltText { get; set; } = string.Empty;
}