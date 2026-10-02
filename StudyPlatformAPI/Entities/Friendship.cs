using System;

namespace StudyPlatformAPI.Entities;

public class Friendship
{
    public int Id { get; set; }

    public int RequesterId { get; set; }
    public User Requester { get; set; } = null!;

    public int AddresseeId { get; set; }
    public User Addressee { get; set; } = null!;

    public int Status { get; set; } = 0; // 0: Bekliyor, 1: Kabul Edildi, 2: Engellendi

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}