using System.ComponentModel.DataAnnotations;

namespace MenStyle.Web.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại")]
    [Display(Name = "Email hoặc số điện thoại")]
    public string LoginIdentifier { get; set; } = string.Empty;
}