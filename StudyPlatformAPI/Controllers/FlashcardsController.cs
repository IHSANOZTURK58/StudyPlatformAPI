// Controllers/FlashcardsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using StudyPlatformAPI.Services;
using StudyPlatformAPI.DTOs;

namespace StudyPlatformAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FlashcardsController : ControllerBase
{
    private readonly IFlashcardService _flashcardService;

    public FlashcardsController(IFlashcardService flashcardService)
    {
        _flashcardService = flashcardService;
    }

    // YARDIMCI METOD: Kimlik Doğrulama
    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }



    // 1. ENDPOINT: Ana Ekran (Dashboard) İçin Kart Sayısı Getirme
    [HttpGet("due-count")]
    public async Task<IActionResult> GetDueCount()
    {
        int userId = GetUserId();
        int count = await _flashcardService.GetDueFlashcardsCountAsync(userId);

        return Ok(new { Count = count });
    }




    // 2. ENDPOINT: Çalışma Ekranı İçin Kartları Getirme
    [HttpGet("due")]
    public async Task<IActionResult> GetDueFlashcards([FromQuery] int limit = 20)
    {
        int userId = GetUserId();
        var cards = await _flashcardService.GetDueFlashcardsAsync(userId, limit);

        return Ok(cards);
    }





    // 3. ENDPOINT: Algoritmayı Tetikleme (Kartı Değerlendirme)
    [HttpPost("{id}/review")]
    public async Task<IActionResult> ReviewFlashcard(int id, [FromBody] ReviewFlashcardDto request)
    {
        int userId = GetUserId();

        if (request.Quality < 0 || request.Quality > 5)
        {
            return BadRequest("Kalite puanı 0 ile 5 arasında olmalıdır.");
        }

        bool result = await _flashcardService.ReviewFlashcardAsync(id, userId, request.Quality);

        if (!result)
        {
            return NotFound("Kart bulunamadı veya size ait değil.");
        }

        return Ok(new { Message = "Kart algoritması başarıyla güncellendi." });
    }
}