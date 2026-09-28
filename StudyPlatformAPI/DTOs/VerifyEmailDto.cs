namespace StudyPlatformAPI.DTOs;

public class VerifyEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}