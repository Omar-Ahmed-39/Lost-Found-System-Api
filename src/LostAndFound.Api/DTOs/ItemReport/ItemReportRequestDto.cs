using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.ItemReports;

public class ItemReportRequestDto
{
    public enReportType ReportType { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public enConditionType ConditionType { get; set; }
    public DateTime DateReported { get; set; }
    public string Description { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public int CategoryId { get; set; }
}