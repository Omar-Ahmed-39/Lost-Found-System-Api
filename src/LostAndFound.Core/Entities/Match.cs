namespace LostAndFound.Core.Entities;

public class Match
{
    public int Id { get; set; }
    public float MatchScore { get; set; }
    public string MatchStatus { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    //Foreign key
    public int LostId { get; set; }
    public int FounId { get; set; }
    public int MatchedBy { get; set; }

    //Navigation Key
    public ItemReport LostItem { get; set; } = default!;
    public ItemReport FoundItem { get; set; } = default!;
    public User MatchedByUser { get; set; } = default!;
}