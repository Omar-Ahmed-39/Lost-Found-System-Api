using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.Claims;

public class MyClaimDto
{
    public int Id { get; set; }
    public string ClaimCode { get; set; } = string.Empty;
    public int ReportId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public DateTime ClaimDate { get; set; }
    public enApprovalStatus ApprovalStatus { get; set; }
}