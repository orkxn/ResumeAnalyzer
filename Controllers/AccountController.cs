using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace ResumeAnalyzer.Controllers
{
    [EnableRateLimiting("GeneralPolicy")]
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController(AuthService _authService)
        {
            this._authService = _authService;
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

            var serviceResult = await _authService.LoginAsync(dto);
            if (!serviceResult.IsSuccess)
            {
                ModelState.AddModelError("", serviceResult.ErrorMessage!);
                return View(dto);
            }

            var result = serviceResult.Data!;
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

            var serviceResult = await _authService.RegisterAsync(dto);
            if (!serviceResult.IsSuccess)
            {
                if (serviceResult.Errors != null && serviceResult.Errors.Count > 0)
                {
                    foreach (var error in serviceResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                else
                {
                    ModelState.AddModelError("", serviceResult.ErrorMessage!);
                }
                return View(dto);
            }

            var result = serviceResult.Data!;
            if (result.Succeeded)
            {
                TempData["RegistrationSuccess"] = "Kayıt başarılı! Giriş yapmak için lütfen bilgilerinizi girin.";
                return RedirectToAction(nameof(Login), new { returnUrl = returnUrl });
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

            var serviceResult = await _authService.ForgotPasswordAsync(email);
            if (!serviceResult.IsSuccess)
            {
                ModelState.AddModelError("", serviceResult.ErrorMessage!);
                return View();
            }

            ViewBag.SuccessMessage = "Şifre sıfırlama yönergeleri e-posta adresinize gönderildi.";
            return View();
        }
    }
}
