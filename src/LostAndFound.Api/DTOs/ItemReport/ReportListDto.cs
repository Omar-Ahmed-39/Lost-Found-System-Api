using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.ItemReports;

public class ReportListDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public DateTime DateReported { get; set; }
}