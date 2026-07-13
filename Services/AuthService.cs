using Microsoft.AspNetCore.Identity;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Services.Interface;

namespace ResumeAnalyzer.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<SignInResult> LoginAsync(LoginRequestDto dto)
        {
            return await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, dto.RememberMe, lockoutOnFailure: false);
        }

        public async Task<IdentityResult> RegisterAsync(RegisterRequestDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                // Kayıt başarılı olunca doğrudan giriş yapıyoruz
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return result;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            // Şimdilik sadece kullanıcının var olduğunu doğrulayıp true dönüyoruz.
            // İleride buraya E-posta gönderme mekanizması (IEmailSender) entegre edilebilir.
            return true;
        }
    }
}
