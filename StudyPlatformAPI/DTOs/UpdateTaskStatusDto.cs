// DTOs/UpdateTaskStatusDto.cs
// Neden yazıyoruz: Mobilden sadece güncellenecek olan "Status" bilgisini almak için.
namespace StudyPlatformAPI.DTOs;

public class UpdateTaskStatusDto
{
    public string NewStatus { get; set; } = string.Empty;
}