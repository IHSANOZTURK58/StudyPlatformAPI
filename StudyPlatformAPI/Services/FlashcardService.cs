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
        return await _context.Flashcards
            .CountAsync(f => f.UserId == userId && f.NextReviewDate <= DateTime.UtcNow);
    }

    public async Task<IEnumerable<Flashcard>> GetDueFlashcardsAsync(int userId, int limit = 20)
    {
        return await _context.Flashcards
            .AsNoTracking()
            .Where(f => f.UserId == userId && f.NextReviewDate <= DateTime.UtcNow)
            .OrderBy(f => f.NextReviewDate) 
            .Take(limit)
            .ToListAsync();
    }

    public async Task<bool> ReviewFlashcardAsync(int flashcardId, int userId, int quality)
    {
        var card = await _context.Flashcards
            .FirstOrDefaultAsync(f => f.Id == flashcardId && f.UserId == userId);

        if (card == null) return false;


        if (quality < 3)
        {
            card.RepetitionCount = 0;
            card.IntervalInDays = 1;
        }
        else
        {
            card.RepetitionCount++;

            if (card.RepetitionCount == 1)
            {
                card.IntervalInDays = 1; 
            }
            else if (card.RepetitionCount == 2)
            {
                card.IntervalInDays = 6; 
            }
            else
            {
                card.IntervalInDays = (int)Math.Round(card.IntervalInDays * card.EasinessFactor);
            }
        }

        card.EasinessFactor = card.EasinessFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));

        if (card.EasinessFactor < 1.3)
        {
            card.EasinessFactor = 1.3;
        }

        card.NextReviewDate = DateTime.UtcNow.AddDays(card.IntervalInDays);

        await _context.SaveChangesAsync();
        return true;
    }
}