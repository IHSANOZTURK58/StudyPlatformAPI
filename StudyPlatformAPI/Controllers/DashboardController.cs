// Controllers/DashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using StudyPlatformAPI.Services;

namespace StudyPlatformAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        // Token içinden giriş yapan kullanıcının ID'sini güvenle alıyoruz
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Aşçıya siparişi verip gelen tepsiyi dönüyoruz
        var dashboardData = await _dashboardService.GetUserDashboardAsync(userId);

        return Ok(dashboardData);
    }
}