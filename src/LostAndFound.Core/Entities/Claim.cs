namespace LostAndFound.Core.Entities;

public class Claim : BaseEntity
{

    public DateTime ClaimDate { get; set; } = DateTime.UtcNow;
    public enApprovalStatus ApprovalStatus { get; set; }
    public string Remarks { get; set; } = string.Empty;

    public DateTime? CancelledAt { get; set; }

    // Foreign Keys
    public int UserId { get; set; }
    public int ReportId { get; set; }

    // Navigation Properties
    public User User { get; set; } = default!;
    public ItemReport Report { get; set; } = default!;
    public Handover? Handover { get; set; } = default!;
}