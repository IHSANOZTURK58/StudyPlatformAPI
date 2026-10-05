// Services/KanbanService.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyPlatformAPI.Data;
using StudyPlatformAPI.DTOs;
using StudyPlatformAPI.Entities;

namespace StudyPlatformAPI.Services;

public class KanbanService : IKanbanService
{
    private readonly AppDbContext _context;

    public KanbanService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KanbanTaskDto>> GetTasksByDateAsync(int userId, DateTime date)
    {
        // MÜHENDİSLİK DETAYI (Performans):
        // AsNoTracking() ile RAM'i koruyoruz. Kullanıcı sadece görevlerini listeleyecek,
        // üzerinde anında değişiklik yapıp kaydetmeyeceğiz. Bu yüzden takip mekanizmasını kapatıyoruz.
        return await _context.KanbanTasks
            .AsNoTracking()
            .Where(k => k.UserId == userId && k.TargetDate.Date == date.Date)
            .Select(k => new KanbanTaskDto
            {
                Id = k.Id,
                Title = k.Title,
                Description = k.Description,
                Status = k.Status,
                TargetDate = k.TargetDate,
                DifficultyWeight = k.DifficultyWeight,
                IsDailyBoss = k.IsDailyBoss
            })
            .ToListAsync();
    }

    public async Task<KanbanTaskDto> CreateTaskAsync(int userId, CreateTaskDto taskDto)
    {
        var newTask = new KanbanTask
        {
            UserId = userId,
            Title = taskDto.Title,
            Description = taskDto.Description,
            TargetDate = taskDto.TargetDate.Date,
            DifficultyWeight = taskDto.DifficultyWeight,
            IsDailyBoss = taskDto.IsDailyBoss,
            Status = "Todo" // Varsayılan olarak "Yapılacaklar" sütununa düşer
        };

        _context.KanbanTasks.Add(newTask);
        await _context.SaveChangesAsync();

        return new KanbanTaskDto
        {
            Id = newTask.Id,
            Title = newTask.Title,
            Description = newTask.Description,
            Status = newTask.Status,
            TargetDate = newTask.TargetDate,
            DifficultyWeight = newTask.DifficultyWeight,
            IsDailyBoss = newTask.IsDailyBoss
        };
    }

    public async Task<bool> UpdateTaskStatusAsync(int taskId, int userId, string newStatus)
    {
        // Önce görevi ve bu görevin sahibini veritabanından çekiyoruz (User tablosunu Include ederek)
        var task = await _context.KanbanTasks
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Id == taskId && k.UserId == userId);

        if (task == null) return false;

        // MÜHENDİSLİK DETAYI (Business Logic - Oyunlaştırma Mekaniği):
        // Servis katmanının neden bu kadar önemli olduğunu burada anlıyoruz. 
        // Sadece bir yazıyı (Todo -> Done) değiştirmiyoruz. Aynı anda sistemin kurallarını işletiyoruz.

        if (newStatus == "Done" && task.Status != "Done")
        {
            // Görev yeni bitirildiyse XP Hesapla!
            // Temel görev puanımız 10 olsun. Zorluk çarpanı ile çarpıyoruz.
            int earnedXP = 10 * task.DifficultyWeight;

            // Eğer Günün Patronu ise %50 Bonus XP ver!
            if (task.IsDailyBoss)
            {
                earnedXP += (int)(earnedXP * 0.5);
            }

            // Kullanıcının toplam hesabına bu XP'yi ekle
            task.User.TotalXP += earnedXP;
        }
        else if (task.Status == "Done" && newStatus != "Done")
        {
            // Kullanıcı görevi "Bitti" sütunundan yanlışlıkla geri alırsa ("Todo"ya çekerse) hile yapmaması için XP'yi geri al!
            int lostXP = 10 * task.DifficultyWeight;
            if (task.IsDailyBoss) lostXP += (int)(lostXP * 0.5);

            task.User.TotalXP -= lostXP;

            // XP'nin eksiye düşmesini engelle
            if (task.User.TotalXP < 0) task.User.TotalXP = 0;
        }

        // Görevin yeni durumunu (Örn: InProgress) güncelle
        task.Status = newStatus;

        // MÜHENDİSLİK DETAYI (Transaction Güvenliği):
        // SaveChangesAsync() arka planda bir "Transaction" başlatır. 
        // Yani görevin durumu güncellenirken sunucu çökerse, adama bedavadan XP vermez. İkisini aynı anda güvenle kaydeder.
        await _context.SaveChangesAsync();

        return true;
    }
}