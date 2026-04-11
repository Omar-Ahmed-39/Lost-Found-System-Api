namespace LostAndFound.Api.DTOs.Feedback;

public class CreateFeedbackDto
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Rating { get; set; }
}
