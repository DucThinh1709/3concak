using System.ComponentModel.DataAnnotations;

namespace MenStyle.Web.ViewModels;

public sealed class VerifyOtpViewModel
{
    [Required]
    public Guid RequestId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải gồm đúng 6 chữ số")]
    [Display(Name = "Mã OTP")]
    public string OtpCode { get; set; } = string.Empty;

    public string MaskedEmail { get; set; } = string.Empty;

    public int RemainingSeconds { get; set; }

    public int RemainingAttempts { get; set; }
}
