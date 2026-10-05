// Services/IKanbanService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudyPlatformAPI.DTOs;

namespace StudyPlatformAPI.Services;

public interface IKanbanService
{
    // Belirli bir günün görevlerini getirir (Örn: Bugünün planı)
    Task<IEnumerable<KanbanTaskDto>> GetTasksByDateAsync(int userId, DateTime date);

    // Yeni görev ekler
    Task<KanbanTaskDto> CreateTaskAsync(int userId, CreateTaskDto taskDto);

    // Görevi sürükle-bırak yaptığımızda durumunu günceller ve eğer bittiyse (Done) XP verir!
    Task<bool> UpdateTaskStatusAsync(int taskId, int userId, string newStatus);
}