// Services/DashboardService.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudyPlatformAPI.Data;
using StudyPlatformAPI.DTOs;

namespace StudyPlatformAPI.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetUserDashboardAsync(int userId)
    {
        // Neden yazıyoruz: Saat ve dakikaları çöpe atıp sadece "Bugün" (Örn: 5 Ekim 2026 00:00:00) 
        // tarihini alıyoruz ki SQL sorgusunda büyük/küçük karşılaştırması doğru çalışsın.
        var today = DateTime.UtcNow.Date;

        // 1. Kullanıcının Toplam XP'si
        // Sadece tek bir sütun (TotalXP) okuyacağımız için Select ile SQL'i rahatlatıyoruz.
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.TotalXP })
            .FirstOrDefaultAsync();

        // 2. Bugünün Pomodoro İstatistikleri (Sayısal)
        // Neden CountAsync?: Bütün seansları RAM'e çekip (.ToList().Count) yapmak yerine 
        // SQL'den sadece direkt sayıyı (Örn: 5) getirmesini istiyoruz. 
        var todayPomodorosCount = await _context.PomodoroSessions
            .Where(p => p.UserId == userId && p.StartTime >= today && p.IsCompleted == true)
            .CountAsync();

        // 3. Bekleyen Flashcard Sayısı
        var dueFlashcardsCount = await _context.Flashcards
            .CountAsync(f => f.UserId == userId && f.NextReviewDate <= DateTime.UtcNow);

        // 4. Bugünün Görevleri (Kanban - Mobil Ekran İçin Hafifletilmiş Hali)
        var todayTasks = await _context.KanbanTasks
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.TargetDate.Date == today)
            .Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,

                // MÜHENDİSLİK DETAYI (BFF Mantığı - Veri Dönüştürme):
                // Veritabanında görev durumu "Todo", "InProgress", "Done" olarak tutuluyor.
                // Ama ana ekranda (Dashboard) mobil uygulamanın sadece "Bu görev bitti mi bitmedi mi?" 
                // bilgisine (True/False) ihtiyacı var. Arka planda (Status == "Done") kontrolü yapıp 
                // mobildeki IsCompleted boolean (True/False) değerine anında çeviriyoruz.
                IsCompleted = (t.Status == "Done"),

                IsDailyBoss = t.IsDailyBoss
            })
            .ToListAsync();

        // 5. Her şeyi tek bir tepside birleştirip (DTO) Controller'a gönder.
        return new DashboardDto
        {
            TotalUserXP = user?.TotalXP ?? 0,
            TodayCompletedPomodoros = todayPomodorosCount,

            // Basit bir kural: Pomodoro başına 10 XP ekliyoruz.
            TodayEarnedXP = todayPomodorosCount * 10,

            DueFlashcardsCount = dueFlashcardsCount,
            TodayTasks = todayTasks
        };
    }
}