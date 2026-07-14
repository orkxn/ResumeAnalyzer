using Microsoft.AspNetCore.Identity;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<ServiceResult<SignInResult>> LoginAsync(LoginRequestDto dto)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, dto.RememberMe, lockoutOnFailure: false);
                return ServiceResult<SignInResult>.Success(result);
            }
            catch (System.Exception)
            {
                return ServiceResult<SignInResult>.Failure("Giriş işlemi gerçekleştirilirken beklenmeyen bir sistem hatası oluştu. Lütfen bağlantınızı kontrol edip tekrar deneyin.");
            }
        }

        public async Task<ServiceResult<IdentityResult>> RegisterAsync(RegisterRequestDto dto)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (result.Succeeded)
                {
                    return ServiceResult<IdentityResult>.Success(result);
                }

                var errors = new System.Collections.Generic.List<string>();
                foreach (var err in result.Errors)
                {
                    errors.Add(err.Description);
                }
                return ServiceResult<IdentityResult>.Failure("Kayıt işlemi başarısız oldu.", errors);
            }
            catch (System.Exception)
            {
                return ServiceResult<IdentityResult>.Failure("Kayıt işlemi gerçekleştirilirken beklenmeyen bir sistem hatası oluştu. Lütfen bilgilerinizi kontrol edip tekrar deneyin.");
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ServiceResult<bool>> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return ServiceResult<bool>.Failure("Bu e-posta adresi ile kayıtlı bir kullanıcı bulunamadı.");
                }

                // Şimdilik sadece kullanıcının var olduğunu doğrulayıp true dönüyoruz.
                // İleride buraya E-posta gönderme mekanizması (IEmailSender) entegre edilebilir.
                return ServiceResult<bool>.Success(true);
            }
            catch (System.Exception)
            {
                return ServiceResult<bool>.Failure("İşlem sırasında beklenmeyen bir hata oluştu. Lütfen bağlantınızı kontrol edip tekrar deneyin.");
            }
        }
    }
}
