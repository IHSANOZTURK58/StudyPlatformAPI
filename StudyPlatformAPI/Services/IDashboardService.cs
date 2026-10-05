// Services/IDashboardService.cs
using System.Threading.Tasks;
using StudyPlatformAPI.DTOs;

namespace StudyPlatformAPI.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetUserDashboardAsync(int userId);
}