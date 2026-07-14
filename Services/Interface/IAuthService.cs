using Microsoft.AspNetCore.Identity;
using ResumeAnalyzer.DTOs;

namespace ResumeAnalyzer.Services.Interface
{
    public interface IAuthService
    {
        Task<ServiceResult<SignInResult>> LoginAsync(LoginRequestDto dto);
        Task<ServiceResult<IdentityResult>> RegisterAsync(RegisterRequestDto dto);
        Task LogoutAsync();
        Task<ServiceResult<bool>> ForgotPasswordAsync(string email);
    }
}
