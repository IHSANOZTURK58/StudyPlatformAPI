
using Microsoft.EntityFrameworkCore;
using StudyPlatformAPI.Data;
using StudyPlatformAPI.DTOs;
using StudyPlatformAPI.Entities;
using System;
using System.Threading.Tasks;

namespace StudyPlatformAPI.Services
{
    public class PomodoroService : IPomodoroService
    {
        private readonly AppDbContext _context;

        public PomodoroService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> StartSessionAsync(int userId, StartPomodoroDto request)
        {
            var session = new PomodoroSession
            {
                UserId = userId,
                PlannedDurationInMinutes = request.PlannedDurationInMinutes,
                Tag = request.Tag,
                StartTime = DateTime.UtcNow
            };

            _context.PomodoroSessions.Add(session);
            await _context.SaveChangesAsync();

            return session.Id; 
        }

        public async Task<bool> FinishSessionAsync(int userId, FinishPomodoroDto request)
        {
            var session = await _context.PomodoroSessions
                .FirstOrDefaultAsync(p => p.Id == request.SessionId && p.UserId == userId);

            if (session == null)
                return false; 

            session.EndTime = DateTime.UtcNow;
            session.IsCompleted = request.IsCompleted;

            // Eğer pomodoro başarıyla bittiyse (pes edilmediyse) kullanıcıya şimdilik doğrudan XP veriyoruz.
            // Not: Mimari planımıza göre ilerleyen aşamalarda bu XP verme işlemini API'yi yormamak için RabbitMQ'ya taşıyacağız.
            if (request.IsCompleted)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TotalXP += 10; // Her başarılı oturum için şimdilik 10 XP
                    // İleride burada seviye atlama (Level up) mantığı da eklenecek
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}