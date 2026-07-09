using System.ComponentModel.DataAnnotations;

namespace MenStyle.Web.ViewModels;

public class ProfileViewModel
{
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    [Display(Name = "Số điện thoại")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn giới tính")]
    [Display(Name = "Giới tính")]
    public string Gender { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}