using Microsoft.AspNetCore.Identity;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailSenderService _emailSender;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EmailSenderService emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
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

        public async Task<ServiceResult<bool>> ForgotPasswordAsync(string email, string callbackUrl)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Güvenlik gereği kullanıcı yoksa bile "Gönderildi" demek iyi bir pratiktir, 
                    // fakat kullanıcı yönlendirmesi için burada hata dönmek istiyorsak dönebiliriz.
                    return ServiceResult<bool>.Failure("Bu e-posta adresi ile kayıtlı bir kullanıcı bulunamadı.");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // callbackUrl içine token ve email parametrelerini ekleyelim
                var delimiter = callbackUrl.Contains("?") ? "&" : "?";
                var resetLink = $"{callbackUrl}{delimiter}token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

                var subject = "CV Analiz - Şifre Sıfırlama Talebi";
                var body = $@"
                    <div style='font-family: sans-serif; max-width: 500px; margin: 0 auto; padding: 20px; border: 1px solid #e5e7eb; rounded-xl;'>
                        <h2 style='color: #1e3a8a; margin-bottom: 20px;'>Şifrenizi mi Unuttunuz?</h2>
                        <p style='color: #374151; font-size: 14px; line-height: 1.5;'>CV Analiz hesabınızın şifresini sıfırlamak için bir talepte bulundunuz. Aşağıdaki butona tıklayarak şifrenizi sıfırlayabilirsiniz:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #2563eb; color: #ffffff; padding: 12px 24px; text-decoration: none; font-weight: 600; border-radius: 8px; display: inline-block;'>Şifremi Sıfırla</a>
                        </div>
                        <p style='color: #6b7280; font-size: 12px; line-height: 1.5;'>Eğer bu talebi siz yapmadıysanız, bu e-postayı güvenle yoksayabilirsiniz. Şifre sıfırlama linki güvenlik nedeniyle sınırlı bir süre için geçerlidir.</p>
                    </div>";

                await _emailSender.SendEmailAsync(email, subject, body);
                return ServiceResult<bool>.Success(true);
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine($"ForgotPassword failed: {ex}");
                return ServiceResult<bool>.Failure("Şifre sıfırlama e-postası gönderilirken beklenmeyen bir sistem hatası oluştu. Lütfen daha sonra tekrar deneyin.");
            }
        }

        public async Task<ServiceResult<IdentityResult>> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return ServiceResult<IdentityResult>.Failure("Kullanıcı bulunamadı.");
                }

                var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
                if (result.Succeeded)
                {
                    return ServiceResult<IdentityResult>.Success(result);
                }

                var errors = new System.Collections.Generic.List<string>();
                foreach (var err in result.Errors)
                {
                    errors.Add(err.Description);
                }
                return ServiceResult<IdentityResult>.Failure("Şifre sıfırlama başarısız oldu.", errors);
            }
            catch (System.Exception)
            {
                return ServiceResult<IdentityResult>.Failure("Şifre sıfırlanırken beklenmeyen bir hata oluştu.");
            }
        }
    }
}
