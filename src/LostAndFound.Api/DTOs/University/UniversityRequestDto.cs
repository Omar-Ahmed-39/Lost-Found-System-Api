using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Api.DTOs.Universities;

public class UniversityRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "University name cannot be empty.")]
    [MaxLength(200, ErrorMessage = "University name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;
}