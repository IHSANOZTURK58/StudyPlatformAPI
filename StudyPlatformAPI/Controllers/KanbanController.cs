// Controllers/KanbanController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using StudyPlatformAPI.DTOs;
using StudyPlatformAPI.Services;

namespace StudyPlatformAPI.Controllers;

[Authorize] // Sadece giriş yapmış, elinde JWT olanlar erişebilir
[ApiController]
[Route("api/[controller]")]
public class KanbanController : ControllerBase
{
    private readonly IKanbanService _kanbanService;

    public KanbanController(IKanbanService kanbanService)
    {
        _kanbanService = kanbanService;
    }

    // YARDIMCI METOD: Token'dan ID çekme (Güvenlik)
    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    // 1. ENDPOINT: Belirli bir günün görevlerini getirir
    // GET: /api/kanban?date=2026-10-05
    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] DateTime date)
    {
        int userId = GetUserId();

        // Eğer mobilden tarih gelmezse, bugünün görevlerini getir
        if (date == default)
        {
            date = DateTime.UtcNow.Date;
        }

        var tasks = await _kanbanService.GetTasksByDateAsync(userId, date);
        return Ok(tasks);
    }

    // 2. ENDPOINT: Yeni görev ekler
    // POST: /api/kanban
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto request)
    {
        int userId = GetUserId();

        // MÜHENDİSLİK DETAYI (Validasyon):
        // Başlık boş gelirse veritabanına kaydetmeden kapıdan çeviriyoruz.
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Görev başlığı boş olamaz.");
        }

        var createdTask = await _kanbanService.CreateTaskAsync(userId, request);
        return Ok(createdTask); // Mobilde anında ekranda göstermesi için oluşturulan görevi geri dönüyoruz
    }

    // 3. ENDPOINT: Görevin durumunu (Todo, InProgress, Done) günceller
    // PATCH: /api/kanban/{id}/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTaskStatusDto request)
    {
        int userId = GetUserId();

        // Geçerli bir durum gönderilmiş mi kontrolü (Güvenlik)
        if (request.NewStatus != "Todo" && request.NewStatus != "InProgress" && request.NewStatus != "Done")
        {
            return BadRequest("Geçersiz görev durumu gönderildi.");
        }

        bool isUpdated = await _kanbanService.UpdateTaskStatusAsync(id, userId, request.NewStatus);

        if (!isUpdated)
        {
            return NotFound("Görev bulunamadı veya size ait değil.");
        }

        return Ok(new { Message = "Görev durumu başarıyla güncellendi." });
    }
}