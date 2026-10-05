// DTOs/DashboardDto.cs
// Neden yazıyoruz: Sunucudan mobil uygulamaya gidecek olan "Tek Tepsi (Kutu)". 
// İçinde Entity (Veritabanı tablosu) barındırmıyoruz ki gereksiz şifreler, ID'ler veya 
// arka plan verileri mobil uygulamaya sızmasın (Veri Gizliliği / Data Privacy).

using System.Collections.Generic;

namespace StudyPlatformAPI.DTOs;

public class DashboardDto
{
    // 1. Kısım: Pomodoro ve Profil Özeti
    public int TodayCompletedPomodoros { get; set; }
    public int TodayEarnedXP { get; set; }
    public int TotalUserXP { get; set; } 



    // 2. Kısım: Flashcard (Aralıklı Tekrar) Özeti
    public int DueFlashcardsCount { get; set; }




    // 3. Kısım: Günün Planı (Kanban)
    public List<TaskItemDto> TodayTasks { get; set; } = new List<TaskItemDto>();
}

public class TaskItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsDailyBoss { get; set; } 
}