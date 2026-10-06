// Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using StudyPlatformAPI.DTOs;
using StudyPlatformAPI.Services;

namespace StudyPlatformAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
    {
        var result = await _authService.VerifyEmailAsync(request);
        return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.IsSuccess) return BadRequest(result.Message);

        return Ok(new { Token = result.Token, Message = result.Message });
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto request)
    {
        var result = await _authService.ResendVerificationAsync(request);
        return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        // Bu metot veritabanına gitmediği için (sadece token'ı okuduğu için) 
        // doğrudan Controller'da kalması mimari açıdan en doğrusudur.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            Message = "Kilitli alana girmeyi başardınız!",
            Id = userId,
            KullaniciAdi = username,
            Rol = role
        });
    }
}