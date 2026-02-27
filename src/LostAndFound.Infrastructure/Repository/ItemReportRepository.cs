using LostAndFound.Core.Enums;

namespace LostAndFound.Infrastructure.Repository;

public class ItemReportRepository : GenericRepository<ItemReport>, IItemReportRepository
{
    private readonly ApplicationDbContext _context;

    public ItemReportRepository(ApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<bool> UpdateReportAsync(
        ItemReport updatedReport,
        int userId,
        bool isAdmin)
    {
        var report = await _context.ItemReports
            .FirstOrDefaultAsync(r => r.Id == updatedReport.Id);

        if (report == null)
            return false;

        if (report.StatusType == enStatusType.Closed && !isAdmin)
            return false;

        if (report.UserId != userId && !isAdmin)
            return false;

        // Only allow updating certain fields
        report.ReportType = updatedReport.ReportType;
        report.ItemName = updatedReport.ItemName;
        report.Color = updatedReport.Color;
        report.Description = updatedReport.Description;
        report.LocationId = updatedReport.LocationId;
        report.CategoryId = updatedReport.CategoryId;

        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReportAsync(
        int reportId,
        int userId,
        bool isAdmin)
    {
        var report = await _context.ItemReports
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
            return false;

        if (report.StatusType == enStatusType.Closed)
            return false;

        if (report.UserId != userId && !isAdmin)
            return false;

        _context.ItemReports.Remove(report);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CloseReportAsync(int reportId)
    {
        var report = await _context.ItemReports
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
            return false;

        report.StatusType = enStatusType.Closed;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}