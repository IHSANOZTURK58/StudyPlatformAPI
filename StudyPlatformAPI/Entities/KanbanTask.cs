using System;

namespace StudyPlatformAPI.Entities;

public class KanbanTask
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    // BENİM EKLEDİĞİM: İsteğe bağlı detay/açıklama alanı
    public string? Description { get; set; }

    public string Status { get; set; } = "Todo"; // "Todo", "InProgress", "Done"
    public string TaskType { get; set; } = "Daily";
    public DateTime TargetDate { get; set; } = DateTime.UtcNow.Date;

    // Alarm Sistemi
    public bool IsReminderSet { get; set; } = false;
    public TimeSpan? ReminderTime { get; set; }

    // Oyunlaştırma
    public int DifficultyWeight { get; set; } = 1;
    public bool IsDailyBoss { get; set; } = false;
}