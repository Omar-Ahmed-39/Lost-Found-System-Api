using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Api.DTOs.Users;

public class ChangeUserRoleDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
