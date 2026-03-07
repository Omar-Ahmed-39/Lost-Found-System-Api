using LostAndFound.Core.Enums;
using LostAndFound.Core.Filters;

namespace LostAndFound.Infrastructure.Repository;

public class ItemReportRepository : GenericRepository<ItemReport>, IItemReportRepository
{
    private readonly ApplicationDbContext _context;

    public ItemReportRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ItemReport> CreateAsync(ItemReport report)
    {
        report.CreatedAt = DateTime.UtcNow;
        report.StatusType = enStatusType.Open;

        await _context.ItemReports.AddAsync(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task<bool> UpdateAsync(ItemReport report, int userId, bool isAdmin)
    {
        var ExistingReport = await _context.ItemReports.FindAsync(report.Id);

        if (ExistingReport == null)
            return false;

        // Only allow the owner of the report or an admin to update it, User cann't update other reports
        if (!isAdmin && ExistingReport.UserId != userId)
            return false;

        // Cann't update closed reports
        if (ExistingReport.StatusType == enStatusType.Closed)
            return false;

        ExistingReport.ItemName = report.ItemName;
        ExistingReport.Color = report.Color;
        ExistingReport.ConditionType = report.ConditionType;
        ExistingReport.LocationId = report.LocationId;
        ExistingReport.Description = report.Description;
        ExistingReport.ConditionType = report.ConditionType;


        if (isAdmin)
        {
            ExistingReport.AdminNotes = report.AdminNotes;
            ExistingReport.StatusType = report.StatusType;
        }

        ExistingReport.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int reportId, int userId, bool isAdmin)
    {
        var Report = await _context.ItemReports.FindAsync(reportId);

        if (Report == null)
            return false;

        if(!isAdmin && Report.UserId != userId) 
            return false;

        _context.ItemReports.Remove(Report);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelAsync(int reportId, int userId, bool isAdmin)
    {
        var Report = await _context.ItemReports.FindAsync(reportId);

        if (Report == null) 
            return false;

        if (!isAdmin && Report.UserId != userId)
            return false;

        if(Report.StatusType == enStatusType.Closed)
            return false;

        Report.StatusType = enStatusType.Canceled;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ItemReport?> GetByIdAsync(int reportId)
    {
        return await _context.ItemReports
            .Include(r => r.Category)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reportId);
    }

    public async Task<IEnumerable<ItemReport>> GetUserReportsAsync(int userId)
    {
        return await _context.ItemReports
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ItemReport>> FilterAsync(ItemReportFilter filter)
    {
        var query = _context.ItemReports
            .Include(r => r.Category)
            .Include(r => r.User)
            .AsQueryable();

        if(filter.CategoryId.HasValue)
            query = query.Where(r => r.CategoryId == filter.CategoryId);

        if(filter.StatusType.HasValue)
            query = query.Where(r => r.StatusType == filter.StatusType);

        if(filter.ReportType.HasValue)
            query = query.Where(r => r.ReportType == filter.ReportType);

        if(filter.FromDate.HasValue)
            query = query.Where(r => r.CreatedAt >= filter.FromDate);

        if(filter.ToDate.HasValue)
            query = query.Where(r => r.CreatedAt <= filter.ToDate);

        if(!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(r => r.ItemName.Contains(filter.Search) || r.Description.Contains(filter.Search));
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }
}