namespace LostAndFound.Core.Entities;

public class Handover
{
    public int Id { get; set; }
    public enIdType IdType { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public DateTime HandoverDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Foreign key
    public int LocationId { get; set; }
    public int ReceiverUserId { get; set; }
    public int HandedByUserId { get; set; }
    public int ClaimId { get; set; }

    // Navigation properties
    public Location Location { get; set; } = default!;
    public User ReceiverUser { get; set; } = default!;
    public User GivenByUser { get; set; } = default!;
    public Claim Claim { get; set; } = default!;

}
