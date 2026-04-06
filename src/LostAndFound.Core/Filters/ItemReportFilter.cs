namespace LostAndFound.Core.Filters;

public class ItemReportFilter
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? LocationId { get; set; }
    public enStatusType? StatusType { get; set; }
    public enReportType? ReportType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}