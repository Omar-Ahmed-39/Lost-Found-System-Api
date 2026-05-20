using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.ItemReports;

public class UserReportDetailsDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public List<ImageDto> Images { get; set; } = new();
    public enReportType ReportType { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public DateTime DateReported { get; set; }
}