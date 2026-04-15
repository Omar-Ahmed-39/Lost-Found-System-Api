namespace LostAndFound.Core.Entities;

public class Notification : BaseEntity
{

    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    // Navigation property
    public User User { get; set; } = default!;
}