using System;

namespace StudyPlatformAPI.Entities;

public class Document
{
    public int Id { get; set; }

    // Neden yazıyoruz: Bu PDF'i kimin yüklediğini bilmek için User tablosuyla ilişki kuruyoruz.
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string MinioFileUrl { get; set; } = string.Empty; // MinIO'daki indirme adresi
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}