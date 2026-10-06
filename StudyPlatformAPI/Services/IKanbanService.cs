// Services/IKanbanService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudyPlatformAPI.DTOs;

namespace StudyPlatformAPI.Services;

public interface IKanbanService
{
    Task<IEnumerable<KanbanTaskDto>> GetTasksByDateAsync(int userId, DateTime date);

    Task<KanbanTaskDto> CreateTaskAsync(int userId, CreateTaskDto taskDto);

    Task<bool> UpdateTaskStatusAsync(int taskId, int userId, string newStatus);
}