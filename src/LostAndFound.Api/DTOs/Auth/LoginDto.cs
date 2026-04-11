using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Api.DTOs.Auth;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>Firebase Cloud Messaging token for push notifications (optional).</summary>
    public string? FcmToken { get; set; }
}
