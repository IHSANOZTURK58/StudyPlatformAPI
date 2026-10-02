
using StudyPlatformAPI.DTOs;
using System.Threading.Tasks;

namespace StudyPlatformAPI.Services
{
    public interface IPomodoroService
    {
        Task<int> StartSessionAsync(int userId, StartPomodoroDto request);
        Task<bool> FinishSessionAsync(int userId, FinishPomodoroDto request);
    }
}