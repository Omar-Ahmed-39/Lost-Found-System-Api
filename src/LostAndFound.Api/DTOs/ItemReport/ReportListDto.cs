using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.ItemReports;

public class ReportListDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public DateTime DateReported { get; set; }
    public enReportType ReportType { get; set; }
}