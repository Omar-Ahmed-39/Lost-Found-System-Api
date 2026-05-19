using System.ComponentModel.DataAnnotations;
using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.ItemReports;

public class ItemReportRequestDto
{
    [Required]
    public enReportType ReportType { get; set; }

    [Required]
    public string ItemName { get; set; } = string.Empty;

    public string? Color { get; set; }

    [Required]
    public enConditionType ConditionType { get; set; }

    [Required]
    public DateTime DateReported { get; set; }

    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A valid location is required.")]
    public int LocationId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A valid category is required.")]
    public int CategoryId { get; set; }
    public List<IFormFile>? Images { get; set; }
}