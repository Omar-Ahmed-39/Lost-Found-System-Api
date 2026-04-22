using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Api.DTOs.Categories;

public class CategoryRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Category name cannot be empty.")]
    [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;
}
