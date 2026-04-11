using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.Claims;

public class ClaimResponseDto
{
    public int Id { get; set; }
    public string ClaimCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ClaimantName { get; set; } = string.Empty;
    public DateTime ClaimDate { get; set; }
    public double? MatchScore { get; set; }
    public enApprovalStatus ApprovalStatus { get; set; }
}