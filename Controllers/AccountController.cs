using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Services.Interface;

namespace ResumeAnalyzer.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto dto, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var result = await _authService.LoginAsync(dto);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyin.");
                }
                else
                {
                    ModelState.AddModelError("", "Geçersiz e-posta veya şifre.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Giriş işlemi gerçekleştirilirken beklenmeyen bir sistem hatası oluştu. Lütfen bağlantınızı kontrol edip tekrar deneyin.");
            }

            return View(dto);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestDto dto, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var result = await _authService.RegisterAsync(dto);

                if (result.Succeeded)
                {
                    TempData["RegistrationSuccess"] = "Kayıt başarılı! Giriş yapmak için lütfen bilgilerinizi girin.";
                    return RedirectToAction(nameof(Login), new { returnUrl = returnUrl });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Kayıt işlemi gerçekleştirilirken beklenmeyen bir sistem hatası oluştu. Lütfen bilgilerinizi kontrol edip tekrar deneyin.");
            }

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Lütfen e-posta adresinizi girin.");
                return View();
            }

            try
            {
                var result = await _authService.ForgotPasswordAsync(email);
                if (result)
                {
                    ViewBag.SuccessMessage = "Şifre sıfırlama yönergeleri e-posta adresinize gönderildi.";
                }
                else
                {
                    // Güvenlik nedeniyle, e-posta bulunmasa bile başarılı gibi gösterebiliriz veya kullanıcıya bulunamadı diyebiliriz.
                    // Burada bulunamadığını belirtelim:
                    ModelState.AddModelError("", "Bu e-posta adresi ile kayıtlı bir kullanıcı bulunamadı.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "İşlem sırasında beklenmeyen bir hata oluştu. Lütfen bağlantınızı kontrol edip tekrar deneyin.");
            }

            return View();
        }
    }
}
