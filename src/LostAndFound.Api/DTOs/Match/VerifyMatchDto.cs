using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Api.DTOs.Match;

public class VerifyMatchDto
{
    [Required]
    public bool IsApproved { get; set; }

    /// <summary>Required when IsApproved is false.</summary>
    public string? RejectionReason { get; set; }
}