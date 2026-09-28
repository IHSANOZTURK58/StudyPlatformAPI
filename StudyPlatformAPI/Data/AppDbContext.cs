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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Bire-Çok ilişkiyi burada açıkça konfigüre ediyoruz (Fluent API)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);
    }
}