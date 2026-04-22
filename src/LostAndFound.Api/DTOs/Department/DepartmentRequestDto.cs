using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Api.DTOs.Departments;

public class DepartmentRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Department name cannot be empty.")]
    [MaxLength(150, ErrorMessage = "Department name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "A valid University is required.")]
    public int UniversityId { get; set; }
}