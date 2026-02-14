namespace LostAndFound.Core.Entities;

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    // Navigation properties
    public User User { get; set; } = default!;
    public Role Role { get; set; } = default!;
}
