using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.ItemReports;

public class AdminReportDetailsDto
{
    public int Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public enReportType ReportType { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public DateTime DateReported { get; set; }
    public string Description { get; set; } = string.Empty;
}