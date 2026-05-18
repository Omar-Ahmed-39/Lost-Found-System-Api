namespace LostAndFound.Api.DTOs.Match;

public class RejectMatchDto
{
    /// <summary>The reason the user is rejecting this match. Required.</summary>
    public string Reason { get; set; } = string.Empty;
}
