// Services/IAuthService.cs
using StudyPlatformAPI.DTOs;
using System.Threading.Tasks;

namespace StudyPlatformAPI.Services;

public interface IAuthService
{               
    Task<(bool IsSuccess, string Message)> RegisterAsync(RegisterDto request);
    Task<(bool IsSuccess, string Message)> VerifyEmailAsync(VerifyEmailDto request);
    Task<(bool IsSuccess, string Message, string Token)> LoginAsync(LoginDto request);
    Task<(bool IsSuccess, string Message)> ResendVerificationAsync(ResendVerificationDto request);
    Task<(bool IsSuccess, string Message)> ForgotPasswordAsync(ForgotPasswordDto request);
    Task<(bool IsSuccess, string Message)> ResetPasswordAsync(ResetPasswordDto request);
}