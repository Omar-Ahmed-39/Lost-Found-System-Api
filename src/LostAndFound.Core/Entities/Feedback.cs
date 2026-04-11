namespace LostAndFound.Core.Entities;

public class Feedback
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Rating { get; set; }

    public bool IsReplied { get; set; } = false;
    public string? AdminReply { get; set; }

    // Foreign key :-
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}