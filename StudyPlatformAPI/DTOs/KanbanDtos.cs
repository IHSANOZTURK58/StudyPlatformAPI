// DTOs/KanbanDtos.cs
// Neden yazıyoruz: Frontend (Mobil) tarafına veritabanı şifrelerini vs. sızdırmadan sadece 
// ihtiyacı olan görev verilerini vermek ve ondan yeni görev alırken kuralları belirlemek için.

using System;

namespace StudyPlatformAPI.DTOs;

// 1. Dışarıya veri yollarken (Okuma işlemi) kullanacağımız DTO
public class KanbanTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public int DifficultyWeight { get; set; }
    public bool IsDailyBoss { get; set; }
}

// 2. Yeni bir görev oluştururken mobilden bize gelecek olan DTO
public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime TargetDate { get; set; }
    public int DifficultyWeight { get; set; } = 1;
    public bool IsDailyBoss { get; set; } = false;
}