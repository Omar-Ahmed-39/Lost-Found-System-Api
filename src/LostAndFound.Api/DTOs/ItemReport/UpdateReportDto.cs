using LostAndFound.Core.Enums;

public class UpdateReportDto
{
    public enReportType? ReportType { get; set; }
    public string? ItemName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public enConditionType? ConditionType { get; set; }
    public DateTime? DateReported { get; set; }
    public string? Description { get; set; }
    public int? LocationId { get; set; }
    public int? CategoryId { get; set; }

    public List<IFormFile>? NewImages { get; set; }
    public List<int>? DeletedImageIds { get; set; }
}