namespace StudyPlatformAPI.Entities;

public class PageNote
{
    public int Id { get; set; }

    // Neden yazıyoruz: Bu notun hangi PDF dokümanına ait olduğunu belirtmek için Document tablosuyla ilişki kuruyoruz.
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public int PageNumber { get; set; } // Hangi sayfaya not alındı?
    public string Content { get; set; } = string.Empty; // Notun metni

    // Neden yazıyoruz: Vize haftası sadece "#vize" etiketli sayfa notlarını filtreleyebilmek için.
    public string? Tag { get; set; }
}