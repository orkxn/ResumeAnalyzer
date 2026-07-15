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

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

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
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result.IsLockedOut
                ? "Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyin."
                : "Geçersiz e-posta veya şifre.");

            return View(dto);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

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
                if (serviceResult.Errors?.Count > 0)
                    foreach (var error in serviceResult.Errors)
                        ModelState.AddModelError("", error);
                else
                    ModelState.AddModelError("", serviceResult.ErrorMessage!);
                return View(dto);
            }

            if (serviceResult.Data!.Succeeded)
            {
                TempData["RegistrationSuccess"] = "Kayıt başarılı! Giriş yapmak için lütfen bilgilerinizi girin.";
                return RedirectToAction(nameof(Login), new { returnUrl });
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
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Lütfen e-posta adresinizi girin.");
                return View();
            }

            var callbackUrl = Url.Action("ResetPassword", "Account", null, Request.Scheme);
            if (string.IsNullOrEmpty(callbackUrl))
            {
                ModelState.AddModelError("", "Geri dönüş URL'i oluşturulamadı.");
                return View();
            }

            var serviceResult = await _authService.ForgotPasswordAsync(email, callbackUrl);
            if (!serviceResult.IsSuccess)
            {
                ModelState.AddModelError("", serviceResult.ErrorMessage!);
                return View();
            }

            ViewBag.SuccessMessage = "Şifre sıfırlama yönergeleri e-posta adresinize gönderildi.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string? token = null, string? email = null)
        {
            if (token == null || email == null)
            {
                return RedirectToAction("Login");
            }
            var model = new ResetPasswordRequestDto { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var serviceResult = await _authService.ResetPasswordAsync(dto);
            if (!serviceResult.IsSuccess)
            {
                if (serviceResult.Errors?.Count > 0)
                    foreach (var error in serviceResult.Errors)
                        ModelState.AddModelError("", error);
                else
                    ModelState.AddModelError("", serviceResult.ErrorMessage!);
                return View(dto);
            }

            TempData["RegistrationSuccess"] = "Şifreniz başarıyla sıfırlandı. Yeni şifrenizle giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }
    }
}
