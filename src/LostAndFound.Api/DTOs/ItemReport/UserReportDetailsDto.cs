using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.ItemReports;

public class UserReportDetailsDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public List<ImageDto> Images { get; set; } = new();
    public string ReportType { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public DateTime DateReported { get; set; }
    public string Description { get; set; } = string.Empty;
}