using LostAndFound.Core.Enums;

namespace LostAndFound.Core.Filters;

public class ClaimFilter
{
    public string? Search { get; set; }
    public enApprovalStatus? ApprovalStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}