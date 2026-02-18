namespace LostAndFound.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime Created { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ItemReport> Reports { get; set; } = new List<ItemReport>();
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    public ICollection<Handover> HandledBy { get; set; } = new List<Handover>();
    public ICollection<Handover> ReceivedHandovers { get; set; } = new List<Handover>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}