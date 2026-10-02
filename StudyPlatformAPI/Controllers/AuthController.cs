using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudyPlatformAPI.Data;
using StudyPlatformAPI.DTOs;
using StudyPlatformAPI.Entities;
using StudyPlatformAPI.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography; // DÜZELTME: Güvenli rastgele sayı üretimi için eklendi
using System.Text;
using System.Threading.Tasks;

namespace StudyPlatformAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (userExists)
        {
            return BadRequest("Bu e-posta adresi zaten kullanılıyor.");
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // DÜZELTME: Güvenli rastgele sayı üretimi (RandomNumberGenerator) ve üst sınır (1000000) düzeltmesi
        string verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            RoleId = 1,
            IsEmailVerified = false,
            VerificationCode = verificationCode,
            VerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        string mailBody = $"Study Platform'a hoş geldiniz! Hesabınızı doğrulamak için kodunuz: {verificationCode} (Bu kod 15 dakika geçerlidir.)";
        await _emailService.SendEmailAsync(newUser.Email, "Study Platform - Hesap Doğrulama", mailBody);

        return Ok("Hesap oluşturuldu. Aktifleştirmek için e-postanıza gelen kodu girin.");
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }

        if (user.IsEmailVerified)
        {
            return BadRequest("Bu hesap zaten doğrulanmış.");
        }

        if (user.VerificationCode != request.Code)
        {
            return BadRequest("Hatalı doğrulama kodu.");
        }

        if (user.VerificationCodeExpiresAt < DateTime.UtcNow)
        {
            return BadRequest("Doğrulama kodunun süresi dolmuş. Lütfen yeni kod isteyin.");
        }

        user.IsEmailVerified = true;
        user.VerificationCode = null;
        user.VerificationCodeExpiresAt = null;

        await _context.SaveChangesAsync();

        return Ok("Hesabınız başarıyla doğrulandı. Artık giriş yapabilirsiniz.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return BadRequest("Kullanıcı adı veya şifre hatalı.");
        }

        if (!user.IsEmailVerified)
        {
            return BadRequest("Lütfen önce e-posta adresinizi doğrulayın.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return BadRequest("Kullanıcı adı veya şifre hatalı.");
        }

        string token = CreateToken(user);

        return Ok(new
        {
            Token = token,
            Message = "Giriş başarılı"
        });
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }

        if (user.IsEmailVerified)
        {
            return BadRequest("Bu hesap zaten doğrulanmış.");
        }

        // DÜZELTME: Güvenli rastgele sayı üretimi (RandomNumberGenerator)
        var verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        user.VerificationCode = verificationCode;
        user.VerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(15);

        await _context.SaveChangesAsync();

        string emailBody = $"Study Platform - Yeni doğrulama kodunuz: {verificationCode} (Bu kod 15 dakika boyunca geçerlidir.)";

        await _emailService.SendEmailAsync(
            user.Email,
            "Yeni Doğrulama Kodunuz",
            emailBody
        );

        return Ok("Yeni doğrulama kodu e-posta adresinize gönderildi.");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        // DÜZELTME: Kötü niyetli kişilerin e-posta tespiti yapmasını engellemek için genel mesaj dönülüyor
        if (user == null)
        {
            return Ok("Eğer bu e-posta adresi sistemimizde kayıtlıysa, şifre sıfırlama kodu gönderilmiştir.");
        }

        // DÜZELTME: Güvenli rastgele sayı üretimi (RandomNumberGenerator) ve üst sınır (1000000) düzeltmesi
        var resetCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        user.PasswordResetCode = resetCode;
        user.PasswordResetCodeExpiration = DateTime.UtcNow.AddMinutes(15);
        await _context.SaveChangesAsync();

        var subject = "Şifre Sıfırlama Kodu";
        var body = $"Şifrenizi sıfırlamak için onay kodunuz: {resetCode}";
        await _emailService.SendEmailAsync(user.Email, subject, body);

        // DÜZELTME: Yukarıdaki (user == null) durumu ile birebir aynı mesaj veriliyor
        return Ok("Eğer bu e-posta adresi sistemimizde kayıtlıysa, şifre sıfırlama kodu gönderilmiştir.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }

        if (user.PasswordResetCode != request.Code)
        {
            return BadRequest("Girdiğiniz onay kodu hatalı.");
        }

        if (user.PasswordResetCodeExpiration < DateTime.UtcNow)
        {
            return BadRequest("Bu kodun süresi dolmuş. Lütfen yeni bir kod talep edin.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiration = null;

        await _context.SaveChangesAsync();

        return Ok("Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz.");
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
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

    private string CreateToken(User user)
    {
        // DÜZELTME: Claim'ler birbirinden ayrıldı ve Role claim'i eklendi.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("Username", user.Username),               // İsteğe bağlı özel claim
            new Claim(ClaimTypes.Name, user.Username),          // Standart İsim
            new Claim(ClaimTypes.Role, user.RoleId.ToString())  // Standart Rol (GetProfile metodu artık bunu bulabilecek)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("Jwt:Key").Value!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            issuer: _configuration.GetSection("Jwt:Issuer").Value,
            audience: _configuration.GetSection("Jwt:Audience").Value,
            claims: claims,
            expires: DateTime.Now.AddMonths(1),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }
}