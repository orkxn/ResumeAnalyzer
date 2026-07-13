using Microsoft.AspNetCore.Identity;
using ResumeAnalyzer.DTOs;

namespace ResumeAnalyzer.Services.Interface
{
    public interface IAuthService
    {
        Task<SignInResult> LoginAsync(LoginRequestDto dto);
        Task<IdentityResult> RegisterAsync(RegisterRequestDto dto);
        Task LogoutAsync();
        Task<bool> ForgotPasswordAsync(string email);
    }
}
