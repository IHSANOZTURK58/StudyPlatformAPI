namespace StudyPlatformAPI.Entities;

public class VisualNote
{
    public int Id { get; set; }

    // Neden yazıyoruz: Görsel notun sahibini bilmek için.
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string MinioImageUrl { get; set; } = string.Empty; // Üzerine not alınan resmin bulut adresi

    // Neden yazıyoruz: İğnenin resim üzerindeki konumu (Örn: X=%45, Y=%60)
    public double X_Coordinate { get; set; }
    public double Y_Coordinate { get; set; }

    public string Content { get; set; } = string.Empty; // İğneye tıklanınca açılacak açıklama
    public string? Tag { get; set; }
}