namespace StudyPlatformAPI.Entities;

public class Flashcard
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string FrontSide { get; set; } = string.Empty;
    public string BackSide { get; set; } = string.Empty;

    public DateTime NextReviewDate { get; set; } = DateTime.UtcNow;

    public int RepetitionCount { get; set; } = 0;

    public int IntervalInDays { get; set; } = 0;
    public double EasinessFactor { get; set; } = 2.5;
}