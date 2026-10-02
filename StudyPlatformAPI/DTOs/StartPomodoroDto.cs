namespace StudyPlatformAPI.DTOs
{
    public class StartPomodoroDto
    {
        public int PlannedDurationInMinutes { get; set; } 
        public string? Tag { get; set; } 
    }
}