using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyPlatformAPI.Data;
using StudyPlatformAPI.Entities;

namespace StudyPlatformAPI.Services;

public class FlashcardService : IFlashcardService
{
    private readonly AppDbContext _context;

    public FlashcardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetDueFlashcardsCountAsync(int userId)
    {
        // MÜHENDİSLİK DETAYI (SQL PERFORMANSI VE BELLEK): 
        // Burada .ToList() kullanmıyoruz. Neden? Eğer kullanırsak 5000 kart RAM'e dolar, sonra sayısı sayılır.
        // Entity Framework, CountAsync komutunu gördüğünde arkada doğrudan şu SQL'i oluşturur:
        // "SELECT COUNT(*) FROM Flashcards WHERE UserId = X AND NextReviewDate <= 'BUGÜN'"
        // SQL Server tabloda veri aramaz, sadece bir tam sayı (integer) döndürür. Milisaniyelik bir işlemdir.
        return await _context.Flashcards
            .CountAsync(f => f.UserId == userId && f.NextReviewDate <= DateTime.UtcNow);
    }

    public async Task<IEnumerable<Flashcard>> GetDueFlashcardsAsync(int userId, int limit = 20)
    {
        // MÜHENDİSLİK DETAYI (PAGINATION VE ASNOTRACKING):
        // 1. Take(limit): Öğrencinin o gün 500 tekrarı olsa bile, telefona ilk başta sadece 20'sini yolluyoruz (Pagination mantığı).
        // 2. AsNoTracking(): Bu çok kritik bir EF Core metodudur. Bu kartları sadece ekranda göstereceğimiz için,
        // EF Core'un bu nesneleri bellekte "Acaba değişecek mi?" diye takip etmesini kapatıyoruz. RAM kullanımını %50 azaltır.
        return await _context.Flashcards
            .AsNoTracking()
            .Where(f => f.UserId == userId && f.NextReviewDate <= DateTime.UtcNow)
            .OrderBy(f => f.NextReviewDate) // Gecikmesi en çok olan kart en öne gelsin.
            .Take(limit)
            .ToListAsync();
    }

    public async Task<bool> ReviewFlashcardAsync(int flashcardId, int userId, int quality)
    {
        // quality parametresi: Öğrencinin kartı ne kadar iyi hatırladığı (0 ile 5 arası).
        // 0: Hiç hatırlayamadım -> 5: Kusursuz hatırladım.

        var card = await _context.Flashcards
            .FirstOrDefaultAsync(f => f.Id == flashcardId && f.UserId == userId);

        if (card == null) return false;

        // MÜHENDİSLİK DETAYI (SM-2 ALGORİTMASI):
        // Bu blok, dünyaca ünlü Spaced Repetition (Aralıklı Tekrar) algoritmasının standart matematiksel implementasyonudur.

        if (quality < 3)
        {
            // 0, 1 veya 2 Puan (Unuttu veya çok zorlandı)
            // Ceza: Tekrar serisi (streak) sıfırlanır. Kart 1 gün sonra tekrar sorulur.
            card.RepetitionCount = 0;
            card.IntervalInDays = 1;
        }
        else
        {
            // 3, 4 veya 5 Puan (Hatırladı)
            card.RepetitionCount++;

            if (card.RepetitionCount == 1)
            {
                card.IntervalInDays = 1; // İlk hatırlama: 1 gün sonra tekrarla
            }
            else if (card.RepetitionCount == 2)
            {
                card.IntervalInDays = 6; // İkinci hatırlama: Beyin bunu 6 gün tutabilir
            }
            else
            {
                // Üçüncü ve sonrası: Önceki bekleyiş süresi, Kolaylık Çarpanı (EasinessFactor) ile çarpılarak katlanır (16 gün, 35 gün vb.)
                card.IntervalInDays = (int)Math.Round(card.IntervalInDays * card.EasinessFactor);
            }
        }

        // Kolaylık Çarpanı (EasinessFactor) Güncelleme Formülü:
        // Kart kolaysa (quality 5) çarpan artar, zorsa (quality 3) çarpan yavaşlar.
        card.EasinessFactor = card.EasinessFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));

        // Çarpanın 1.3'ün altına düşmesini engelliyoruz. 
        // Aksi takdirde formül çöker ve kart sürekli öğrencinin önüne düşerek onu bıktırır.
        if (card.EasinessFactor < 1.3)
        {
            card.EasinessFactor = 1.3;
        }

        // Hesaplanan IntervalInDays değerini bugünün (UtcNow) üstüne ekleyerek bir sonraki sınav tarihini belirliyoruz.
        card.NextReviewDate = DateTime.UtcNow.AddDays(card.IntervalInDays);

        await _context.SaveChangesAsync();
        return true;
    }
}