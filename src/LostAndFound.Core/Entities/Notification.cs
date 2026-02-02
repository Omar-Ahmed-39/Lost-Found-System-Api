namespace LostAndFound.Core.Entities;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }

    // Foreign key
    public int UserId { get; set; }

    // Navigation property
    public User User { get; set; } = default!;
}