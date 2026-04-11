using Microsoft.AspNetCore.Identity;

namespace LostAndFound.Core.Entities;

public class User : IdentityUser<int>
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? FcmToken { get; set; }
    public DateTime Created { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ItemReport> Reports { get; set; } = new List<ItemReport>();
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    public ICollection<Handover> HandledBy { get; set; } = new List<Handover>();
    public ICollection<Handover> ReceivedHandovers { get; set; } = new List<Handover>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}