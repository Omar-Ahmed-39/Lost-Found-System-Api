using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Api.DTOs.Users;

public class UpdateUserDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
