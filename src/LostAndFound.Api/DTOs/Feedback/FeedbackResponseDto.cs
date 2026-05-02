namespace LostAndFound.Api.DTOs.Feedback;

public class FeedbackResponseDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool IsReplied { get; set; }
    public string? AdminReply { get; set; }
    
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
}
