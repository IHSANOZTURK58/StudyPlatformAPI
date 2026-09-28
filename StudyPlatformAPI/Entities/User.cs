namespace StudyPlatformAPI.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Oyunlaştırma (Gamification) alanı
    public int TotalXP { get; set; } = 0;
    public int CurrentLevel { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // İlişki (Foreign Key): Kullanıcının bir rolü olmalı
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    //E posta Doğrulama
    public bool IsEmailVerified { get; set; } = false;
    public string? VerificationCode { get; set; }
    public DateTime? VerificationCodeExpiresAt { get; set; }
}