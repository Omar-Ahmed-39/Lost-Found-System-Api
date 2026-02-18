namespace LostAndFound.Core.Entities;

public class Claim
{
    public int Id { get; set; }
    public DateTime ClaimDate { get; set; } = DateTime.UtcNow;
    public enApprovalStatus ApprovalStatus { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }

    // Foreign Keys
    public int UserId { get; set; }
    public int ReportId { get; set; }

    // Navigation Properties
    public User User { get; set; } = default!;
    public ItemReport Report { get; set; } = default!;
    public Handover? Handover { get; set; } = default!;
}