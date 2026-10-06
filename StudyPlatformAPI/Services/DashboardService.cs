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
        var today = DateTime.UtcNow.Date;

        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.TotalXP })
            .FirstOrDefaultAsync();
        var todayPomodorosCount = await _context.PomodoroSessions
            .Where(p => p.UserId == userId && p.StartTime >= today && p.IsCompleted == true)
            .CountAsync();

        var dueFlashcardsCount = await _context.Flashcards
            .CountAsync(f => f.UserId == userId && f.NextReviewDate <= DateTime.UtcNow);

        var todayTasks = await _context.KanbanTasks
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.TargetDate.Date == today)
            .Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,

                IsCompleted = (t.Status == "Done"),

                IsDailyBoss = t.IsDailyBoss
            })
            .ToListAsync();

        return new DashboardDto
        {
            TotalUserXP = user?.TotalXP ?? 0,
            TodayCompletedPomodoros = todayPomodorosCount,

            TodayEarnedXP = todayPomodorosCount * 10,

            DueFlashcardsCount = dueFlashcardsCount,
            TodayTasks = todayTasks
        };
    }
}