
namespace StudyPlatformAPI.Entities;

public class PomodoroSession
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; } 

    public int PlannedDurationInMinutes { get; set; } 
    public bool IsCompleted { get; set; } = false;

    public string? Tag { get; set; }
    public bool IsOfflineSynced { get; set; } = false;
}