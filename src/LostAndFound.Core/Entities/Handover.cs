namespace LostAndFound.Core.Entities;

public class Handover
{
    public int Id { get; set; }
    public enIdType IdType { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public DateTime HandoverDate { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int LocationId { get; set; }
    public int ReceiverUserId { get; set; }
    public int HandedByUserId { get; set; }
    public int ClaimId { get; set; }

    public Location Location { get; set; } = default!;
    public User ReceiverUser { get; set; } = default!;
    public User HandedByUser { get; set; } = default!;
    public Claim Claim { get; set; } = default!;
}