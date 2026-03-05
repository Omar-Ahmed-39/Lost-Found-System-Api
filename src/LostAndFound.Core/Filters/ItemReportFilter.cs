namespace LostAndFound.Core.Filters;

public class ItemReportFilter
{
    public int? CategoryId { get; set; }
    public enStatusType? StatusType { get; set; }
    public enReportType? ReportType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? ToDate { get; set; }
    public String? Search { get; set; }

}
