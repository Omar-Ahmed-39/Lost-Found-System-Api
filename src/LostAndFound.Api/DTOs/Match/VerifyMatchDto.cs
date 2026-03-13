namespace LostAndFound.Api.DTOs.Match;

public class VerifyMatchDto
{
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}