// Services/AuthService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using StudyPlatformAPI.Data;
using StudyPlatformAPI.DTOs;
using StudyPlatformAPI.Entities;

namespace StudyPlatformAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<(bool IsSuccess, string Message)> RegisterAsync(RegisterDto request)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (userExists) return (false, "Bu e-posta adresi zaten kullanılıyor.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
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

        return (true, "Hesap oluşturuldu. Aktifleştirmek için e-postanıza gelen kodu girin.");
    }

    public async Task<(bool IsSuccess, string Message)> VerifyEmailAsync(VerifyEmailDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return (false, "Kullanıcı bulunamadı.");
        if (user.IsEmailVerified) return (false, "Bu hesap zaten doğrulanmış.");
        if (user.VerificationCode != request.Code) return (false, "Hatalı doğrulama kodu.");
        if (user.VerificationCodeExpiresAt < DateTime.UtcNow) return (false, "Doğrulama kodunun süresi dolmuş. Lütfen yeni kod isteyin.");

        user.IsEmailVerified = true;
        user.VerificationCode = null;
        user.VerificationCodeExpiresAt = null;
        await _context.SaveChangesAsync();

        return (true, "Hesabınız başarıyla doğrulandı. Artık giriş yapabilirsiniz.");
    }

    public async Task<(bool IsSuccess, string Message, string Token)> LoginAsync(LoginDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return (false, "Kullanıcı adı veya şifre hatalı.", null!);
        if (!user.IsEmailVerified) return (false, "Lütfen önce e-posta adresinizi doğrulayın.", null!);

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid) return (false, "Kullanıcı adı veya şifre hatalı.", null!);

        string token = CreateToken(user);
        return (true, "Giriş başarılı", token);
    }

    public async Task<(bool IsSuccess, string Message)> ResendVerificationAsync(ResendVerificationDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return (false, "Kullanıcı bulunamadı.");
        if (user.IsEmailVerified) return (false, "Bu hesap zaten doğrulanmış.");

        var verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        user.VerificationCode = verificationCode;
        user.VerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(15);
        await _context.SaveChangesAsync();

        string emailBody = $"Study Platform - Yeni doğrulama kodunuz: {verificationCode} (Bu kod 15 dakika boyunca geçerlidir.)";
        await _emailService.SendEmailAsync(user.Email, "Yeni Doğrulama Kodunuz", emailBody);

        return (true, "Yeni doğrulama kodu e-posta adresinize gönderildi.");
    }

    public async Task<(bool IsSuccess, string Message)> ForgotPasswordAsync(ForgotPasswordDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        // Güvenlik: Kullanıcı olmasa bile aynı mesajı dönüyoruz
        if (user == null) return (true, "Eğer bu e-posta adresi sistemimizde kayıtlıysa, şifre sıfırlama kodu gönderilmiştir.");

        var resetCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        user.PasswordResetCode = resetCode;
        user.PasswordResetCodeExpiration = DateTime.UtcNow.AddMinutes(15);
        await _context.SaveChangesAsync();

        var body = $"Şifrenizi sıfırlamak için onay kodunuz: {resetCode}";
        await _emailService.SendEmailAsync(user.Email, "Şifre Sıfırlama Kodu", body);

        return (true, "Eğer bu e-posta adresi sistemimizde kayıtlıysa, şifre sıfırlama kodu gönderilmiştir.");
    }

    public async Task<(bool IsSuccess, string Message)> ResetPasswordAsync(ResetPasswordDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return (false, "Kullanıcı bulunamadı.");
        if (user.PasswordResetCode != request.Code) return (false, "Girdiğiniz onay kodu hatalı.");
        if (user.PasswordResetCodeExpiration < DateTime.UtcNow) return (false, "Bu kodun süresi dolmuş. Lütfen yeni bir kod talep edin.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiration = null;
        await _context.SaveChangesAsync();

        return (true, "Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz.");
    }

    // JWT Üretim Metodu (Sadece bu sınıf içinde kullanılacağı için private kaldı)
    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("Username", user.Username),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.RoleId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            issuer: _configuration.GetSection("Jwt:Issuer").Value,
            audience: _configuration.GetSection("Jwt:Audience").Value,
            claims: claims,
            expires: DateTime.Now.AddMonths(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}