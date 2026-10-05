using System.Collections.Generic;
using System.Threading.Tasks;
using StudyPlatformAPI.Entities;

namespace StudyPlatformAPI.Services;

public interface IFlashcardService
{
    // Ana ekran için (Sadece sayıyı getirir)
    Task<int> GetDueFlashcardsCountAsync(int userId);

    // Çalışma ekranı için (Kartların kendisini sınırlı sayıda getirir)
    Task<IEnumerable<Flashcard>> GetDueFlashcardsAsync(int userId, int limit = 20);

    // Karta verilen cevaba göre (0-5 arası puan) algoritmayı hesaplayıp günceller
    Task<bool> ReviewFlashcardAsync(int flashcardId, int userId, int quality);
}