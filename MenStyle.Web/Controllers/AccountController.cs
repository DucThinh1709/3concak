using MenStyle.Web.Data;
using MenStyle.Web.Models;
using MenStyle.Web.Services;
using MenStyle.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace MenStyle.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPasswordResetOtpService _otpService;

        public AccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPasswordResetOtpService otpService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _otpService = otpService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string normalizedPhone = NormalizePhoneNumber(model.PhoneNumber);

            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            var phoneExists = await _context.Users
                .AnyAsync(u => u.PhoneNumber == normalizedPhone);

            if (phoneExists)
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại này đã được sử dụng.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = normalizedPhone,
                Address = model.Address,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApplicationUser? user;

            string loginValue = model.LoginIdentifier.Trim();

            if (loginValue.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(loginValue);
            }
            else
            {
                string normalizedPhone = NormalizePhoneNumber(loginValue);

                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone);
            }

            if (user == null)
            {
                ModelState.AddModelError("", "Email, số điện thoại hoặc mật khẩu không đúng.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Email, số điện thoại hoặc mật khẩu không đúng.");
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Gender = user.Gender,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            ModelState.Remove(nameof(model.Email));
            ModelState.Remove(nameof(model.CreatedAt));
            ModelState.Remove(nameof(model.AvatarUrl));

            if (!ModelState.IsValid)
            {
                model.Email = user.Email ?? "";
                model.CreatedAt = user.CreatedAt;
                model.AvatarUrl = user.AvatarUrl;

                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim() ?? "";
            user.Gender = model.Gender?.Trim() ?? "";
            user.Address = model.Address?.Trim() ?? "";

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.AvatarFile), "Chỉ cho phép ảnh .jpg, .jpeg, .png hoặc .webp.");

                    model.Email = user.Email ?? "";
                    model.CreatedAt = user.CreatedAt;
                    model.AvatarUrl = user.AvatarUrl;

                    return View(model);
                }

                if (model.AvatarFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(model.AvatarFile), "Ảnh đại diện không được vượt quá 2MB.");

                    model.Email = user.Email ?? "";
                    model.CreatedAt = user.CreatedAt;
                    model.AvatarUrl = user.AvatarUrl;

                    return View(model);
                }

                var avatarFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");

                if (!Directory.Exists(avatarFolder))
                {
                    Directory.CreateDirectory(avatarFolder);
                }

                var fileName = $"{user.Id}_{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(avatarFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                user.AvatarUrl = $"/images/avatars/{fileName}";
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.Email = user.Email ?? "";
                model.CreatedAt = user.CreatedAt;
                model.AvatarUrl = user.AvatarUrl;

                return View(model);
            }

            TempData["SuccessMessage"] = "Đã lưu thông tin cá nhân.";

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var orders = await _context.CustomerOrders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyOrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var order = await _context.CustomerOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            return phoneNumber
                .Replace(" ", "")
                .Replace("-", "")
                .Replace(".", "")
                .Replace("(", "")
                .Replace(")", "")
                .Trim();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginIdentifier = model.LoginIdentifier.Trim();
            ApplicationUser? user;

            if (loginIdentifier.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(loginIdentifier);
            }
            else
            {
                var normalizedPhone = NormalizePhoneNumber(loginIdentifier);

                user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone, cancellationToken);
            }

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể gửi mã OTP. Vui lòng kiểm tra lại email hoặc số điện thoại đã đăng ký.");

                return View(model);
            }

            var sendResult = await _otpService.CreateOrResendAsync(
                user.Id,
                user.Email,
                user.FullName,
                cancellationToken);

            if (!sendResult.Succeeded)
            {
                if (sendResult.RequestId != Guid.Empty)
                {
                    TempData["ErrorMessage"] = sendResult.Message;

                    return RedirectToAction(nameof(VerifyOtp), new
                    {
                        requestId = sendResult.RequestId
                    });
                }

                ModelState.AddModelError(string.Empty, sendResult.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = sendResult.Message;

            return RedirectToAction(nameof(VerifyOtp), new
            {
                requestId = sendResult.RequestId
            });
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> VerifyOtp(
            Guid requestId,
            CancellationToken cancellationToken)
        {
            if (requestId == Guid.Empty)
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var requestInfo = await _otpService.GetInfoAsync(requestId, cancellationToken);

            if (requestInfo == null)
            {
                TempData["ErrorMessage"] =
                    "Yêu cầu OTP không tồn tại hoặc đã hết hiệu lực. Vui lòng yêu cầu mã mới.";

                return RedirectToAction(nameof(ForgotPassword));
            }

            return View(new VerifyOtpViewModel
            {
                RequestId = requestId,
                MaskedEmail = requestInfo.MaskedEmail,
                RemainingSeconds = requestInfo.RemainingSeconds,
                RemainingAttempts = requestInfo.RemainingAttempts
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(
            VerifyOtpViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await PopulateOtpViewModelAsync(model, cancellationToken);
                return View(model);
            }

            var verifyResult = await _otpService.VerifyAsync(
                model.RequestId,
                model.OtpCode,
                cancellationToken);

            if (verifyResult.Status == OtpVerifyStatus.Succeeded)
            {
                var user = await _userManager.FindByIdAsync(verifyResult.UserId);

                if (user == null)
                {
                    _otpService.InvalidateForUser(verifyResult.UserId);
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản cần đặt lại mật khẩu.";
                    return RedirectToAction(nameof(ForgotPassword));
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                TempData["SuccessMessage"] =
                    "Xác minh OTP thành công. Vui lòng tạo mật khẩu mới.";

                return RedirectToAction(nameof(ResetPassword), new
                {
                    userId = user.Id,
                    token = encodedToken
                });
            }

            var errorMessage = verifyResult.Status switch
            {
                OtpVerifyStatus.Expired =>
                    "Mã OTP đã hết hạn. Vui lòng bấm Gửi lại mã OTP.",

                OtpVerifyStatus.InvalidCode =>
                    $"Mã OTP không đúng. Bạn còn {verifyResult.RemainingAttempts} lần nhập.",

                OtpVerifyStatus.TooManyAttempts =>
                    "Bạn đã nhập sai quá số lần cho phép. Vui lòng gửi lại mã OTP mới.",

                OtpVerifyStatus.AlreadyUsed =>
                    "Mã OTP này đã được sử dụng. Vui lòng tạo yêu cầu mới nếu cần.",

                _ =>
                    "Yêu cầu OTP không tồn tại hoặc đã hết hiệu lực. Vui lòng yêu cầu mã mới."
            };

            ModelState.AddModelError(nameof(model.OtpCode), errorMessage);
            await PopulateOtpViewModelAsync(model, cancellationToken);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(
            Guid requestId,
            CancellationToken cancellationToken)
        {
            if (requestId == Guid.Empty)
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var sendResult = await _otpService.ResendAsync(requestId, cancellationToken);

            if (sendResult.Succeeded)
            {
                TempData["SuccessMessage"] = sendResult.Message;
            }
            else
            {
                TempData["ErrorMessage"] = sendResult.Message;
            }

            return RedirectToAction(nameof(VerifyOtp), new { requestId });
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Login");
            }

            string decodedToken;

            try
            {
                decodedToken = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(model.Token));
            }
            catch (Exception exception) when (
                exception is FormatException or ArgumentException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");

                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (result.Succeeded)
            {
                _otpService.InvalidateForUser(user.Id);
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        private async Task PopulateOtpViewModelAsync(
            VerifyOtpViewModel model,
            CancellationToken cancellationToken)
        {
            var requestInfo = await _otpService.GetInfoAsync(
                model.RequestId,
                cancellationToken);

            if (requestInfo == null)
            {
                model.MaskedEmail = "email đã đăng ký";
                model.RemainingSeconds = 0;
                model.RemainingAttempts = 0;
                return;
            }

            model.MaskedEmail = requestInfo.MaskedEmail;
            model.RemainingSeconds = requestInfo.RemainingSeconds;
            model.RemainingAttempts = requestInfo.RemainingAttempts;
        }
    }
}
