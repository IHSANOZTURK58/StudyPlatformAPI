namespace StudyPlatformAPI.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Free, Premium, Admin

    // Bir rolün birden fazla kullanıcısı olabilir
    public ICollection<User> Users { get; set; } = new List<User>();
}