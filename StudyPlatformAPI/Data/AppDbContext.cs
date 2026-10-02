using Microsoft.EntityFrameworkCore;
using StudyPlatformAPI.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace StudyPlatformAPI.Data;

public class AppDbContext : DbContext
{
    // Constructor (Dependency Injection ile veritabanı ayarlarını almak için)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Veritabanındaki tablolarımız
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<PomodoroSession> PomodoroSessions { get; set; }

    public DbSet<Document> Documents { get; set; }
    public DbSet<PageNote> PageNotes { get; set; }
    public DbSet<VisualNote> VisualNotes { get; set; }

    public DbSet<KanbanTask> KanbanTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Otomatik "Silinmemiş olanları getir" filtresi
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        // 2. Pomodoro için Cascade iptal ediliyor, Restrict yapılıyor
        modelBuilder.Entity<PomodoroSession>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // 3. Dokümanlar için Cascade iptal ediliyor (Kullanıcı soft delete olsa bile dosya kalsın)
        modelBuilder.Entity<Document>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Görsel Notlar için Cascade iptal ediliyor
        modelBuilder.Entity<VisualNote>()
            .HasOne(vn => vn.User)
            .WithMany()
            .HasForeignKey(vn => vn.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Not: PageNote tablosu direkt Document tablosuna bağlı. Dokümanın kendisi silinirse notları da silinsin.
        modelBuilder.Entity<PageNote>()
            .HasOne(pn => pn.Document)
            .WithMany()
            .HasForeignKey(pn => pn.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // 2. Aynı dosyanın içindeki OnModelCreating metodunun içine, diğer tablo ilişkilerinin yanına şu kuralı ekle:
        modelBuilder.Entity<KanbanTask>()
            .HasOne(k => k.User)
            .WithMany()
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}