// Entities/KanbanTask.cs
// Neden yazıyoruz? Günlük görevleri, hedefleri ve yeni eklediğimiz "oyunlaştırma/koçluk" mekaniklerini (Zorluk derecesi ve Günün Patronu) tek bir yerde tutmak için.

using System;

namespace StudyPlatformAPI.Entities;

public class KanbanTask
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Todo";
    public string TaskType { get; set; } = "Daily";
    public DateTime TargetDate { get; set; } = DateTime.UtcNow.Date;

    // Alarm Sistemi
    public bool IsReminderSet { get; set; } = false;
    public TimeSpan? ReminderTime { get; set; }

    public int DifficultyWeight { get; set; } = 1;
    public bool IsDailyBoss { get; set; } = false;
}