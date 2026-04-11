using LostAndFound.Core.Enums;

namespace LostAndFound.Core.Filters;

public class ItemReportFilter
{
    public int? CategoryId { get; set; }
    public int? LocationId { get; set; }
    public enStatusType? StatusType { get; set; }
    public enReportType? ReportType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
}