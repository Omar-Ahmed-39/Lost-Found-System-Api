using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.Claims;

public class ClaimReviewDetailsDto
{
    public int Id { get; set; }
    public string ClaimCode { get; set; } = string.Empty;
    public DateTime ClaimDate { get; set; }
    public enApprovalStatus ApprovalStatus { get; set; }
    public string Remarks { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public DateTime DateReported { get; set; }

    public string ClaimantName { get; set; } = string.Empty;
    public string ClaimantEmail { get; set; } = string.Empty;

    public double? MatchScore { get; set; }

    public List<string> ItemImages { get; set; } = new();
}