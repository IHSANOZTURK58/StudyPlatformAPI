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
            Status = "Todo" 
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
        var task = await _context.KanbanTasks
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Id == taskId && k.UserId == userId);

        if (task == null) return false;


        if (newStatus == "Done" && task.Status != "Done")
        {
            int earnedXP = 10 * task.DifficultyWeight;

            if (task.IsDailyBoss)
            {
                earnedXP += (int)(earnedXP * 0.5);
            }

            task.User.TotalXP += earnedXP;
        }
        else if (task.Status == "Done" && newStatus != "Done")
        {
            int lostXP = 10 * task.DifficultyWeight;
            if (task.IsDailyBoss) lostXP += (int)(lostXP * 0.5);

            task.User.TotalXP -= lostXP;

            if (task.User.TotalXP < 0) task.User.TotalXP = 0;
        }

        task.Status = newStatus;

        await _context.SaveChangesAsync();

        return true;
    }
}