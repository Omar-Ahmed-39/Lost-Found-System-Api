using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.Match;

public class MatchResponseDto
{
    public int Id { get; set; }
    public double MatchScore { get; set; }
    public enMatchStatus Status { get; set; }
    public DateTime MatchDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? RejectionReason { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int LostId { get; set; }
    public int FoundId { get; set; }
    public int MatchedBy { get; set; }
}
