using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.ItemReports;

public class ItemReportResponseDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public enConditionType ConditionType { get; set; }
    public enStatusType StatusType { get; set; }
    public enReportType ReportType { get; set; }
    public DateTime DateReported { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}