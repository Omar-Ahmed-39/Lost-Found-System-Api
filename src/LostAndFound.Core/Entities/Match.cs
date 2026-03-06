namespace LostAndFound.Core.Entities;

public class Match
{
    public int Id { get; private set; }
    public double MatchScore { get; private set; }
    public enMatchStatus Status { get; private set; }
    public DateTime MatchDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public int? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    // Foreign keys
    public int LostId { get; private set; }
    public int FoundId { get; private set; }
    public int MatchedBy { get; private set; }

    // Navigation properties
    public ItemReport LostItem { get; private set; } = default!;
    public ItemReport FoundItem { get; private set; } = default!;
    public User MatchedByUser { get; private set; } = default!;

    // Required by EF Core
    private Match() { }

    /// <summary>
    /// Factory method for creating a new pending Match.
    /// </summary>
    public static Match Create(int lostId, int foundId, double matchScore, int matchedBy)
    {
        return new Match
        {
            LostId = lostId,
            FoundId = foundId,
            MatchScore = matchScore,
            MatchedBy = matchedBy,
            Status = enMatchStatus.Pending,
            MatchDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Approves the match. Returns false if the match is not in a Pending state.
    /// </summary>
    public bool Approve(int adminId)
    {
        if (Status != enMatchStatus.Pending)
            return false;

        Status = enMatchStatus.Confirmed;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return true;
    }

    /// <summary>
    /// Rejects the match. Returns false if the match is not in a Pending state.
    /// </summary>
    public bool Reject(int adminId, string reason)
    {
        if (Status != enMatchStatus.Pending)
            return false;

        if (string.IsNullOrWhiteSpace(reason))
            return false;

        Status = enMatchStatus.Rejected;
        RejectionReason = reason;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return true;
    }
}