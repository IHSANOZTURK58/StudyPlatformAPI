// Controllers/PomodoroController.cs
// Neden yazıyoruz? Service katmanında yazdığımız yetenekleri internete (API'ye) açmak için. 
// [Authorize] etiketi sayesinde sadece geçerli bir JWT Token ile (yani giriş yapmış) gelen kullanıcılar bu işlemleri yapabilecek.
// UserId'yi dışarıdan güvenilmez bir şekilde almak yerine, doğrudan token'ın içinden (ClaimTypes.NameIdentifier) güvenli bir şekilde çekiyoruz.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlatformAPI.DTOs;
using StudyPlatformAPI.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudyPlatformAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class PomodoroController : ControllerBase
    {
        private readonly IPomodoroService _pomodoroService;

        public PomodoroController(IPomodoroService pomodoroService)
        {
            _pomodoroService = pomodoroService;
        }

        [HttpPost("start")]
        public async Task<IActionResult>Start([FromBody] StartPomodoroDto request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized("Geçersiz kullanıcı kimliği.");

            var sessionId = await _pomodoroService.StartSessionAsync(userId, request);

            return Ok(new { Message = "Pomodoro başarıyla başladı.", SessionId = sessionId });
        }

        [HttpPost("finish")]
        public async Task<IActionResult>Finish([FromBody] FinishPomodoroDto request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized("Geçersiz kullanıcı kimliği.");

            var result = await _pomodoroService.FinishSessionAsync(userId, request);

            if (!result)
                return BadRequest("Oturum bulunamadı veya bu işlemi yapmaya yetkiniz yok.");

            return Ok(new { Message = "Pomodoro başarıyla sonlandırıldı." });
        }
    }
}