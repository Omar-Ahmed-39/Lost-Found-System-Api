using System.ComponentModel.DataAnnotations;
using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.Locations;

public class LocationRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Location name cannot be empty.")]
    [MaxLength(150, ErrorMessage = "Location name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public enLocationType LocationType { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "A valid DepartmentId is required.")]
    public int DepartmentId { get; set; }
}
