using MenStyle.Web.Data;
using MenStyle.Web.Models;
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

        public AccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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

            var model = new ProfileViewModel
            {
                Email = user.Email ?? "",
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? "",
                Address = user.Address,
                Gender = user.Gender,
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

            model.Email = user.Email ?? "";
            model.CreatedAt = user.CreatedAt;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var phoneExists = _userManager.Users
                .Any(u => u.PhoneNumber == model.PhoneNumber && u.Id != user.Id);

            if (phoneExists)
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), "Số điện thoại này đã được tài khoản khác sử dụng.");
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber.Trim();
            user.Address = model.Address.Trim();
            user.Gender = model.Gender;
            user.SecurityStamp = Guid.NewGuid().ToString();

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                TempData["SuccessMessage"] = "Thông tin cá nhân đã được lưu thành công.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
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
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.LoginIdentifier);

            if (user == null)
            {
                user = _userManager.Users
                    .FirstOrDefault(u => u.PhoneNumber == model.LoginIdentifier);
            }

            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản phù hợp.";
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return RedirectToAction("ResetPassword", new
            {
                userId = user.Id,
                token = encodedToken
            });
        }

        [HttpGet]
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

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}