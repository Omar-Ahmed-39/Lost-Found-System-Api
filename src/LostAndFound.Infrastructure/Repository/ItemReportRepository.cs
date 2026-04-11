using LostAndFound.Core.Entities;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Filters;
using LostAndFound.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Infrastructure.Repository;

public class ItemReportRepository : GenericRepository<ItemReport>, IItemReportRepository
{
    private readonly ApplicationDbContext _context;

    public ItemReportRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ItemReport> Items, int TotalCount)> GetFilteredAsync(
       ItemReportFilter filter,
       int pageNumber,
       int pageSize)
    {
        IQueryable<ItemReport> query = _context.ItemReports
            .Include(r => r.Category)
            .Include(r => r.Location)
            .Include(r => r.User)
            .Include(r => r.Attachments);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();

            query = query.Where(r =>
                r.ItemName.Contains(search) ||
                r.Description.Contains(search) ||
                r.User.Name.Contains(search) ||
                r.Category.Name.Contains(search) ||
                r.Location.Name.Contains(search));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(r => r.CategoryId == filter.CategoryId.Value);

        if (filter.LocationId.HasValue)
            query = query.Where(r => r.LocationId == filter.LocationId.Value);

        if (filter.StatusType.HasValue)
            query = query.Where(r => r.StatusType == filter.StatusType.Value);

        if (filter.ReportType.HasValue)
            query = query.Where(r => r.ReportType == filter.ReportType.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(r => r.DateReported >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(r => r.DateReported <= filter.ToDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.DateReported)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ItemReport?> GetDetailsAsync(int reportId)
    {
        return await _context.ItemReports
            .Include(r => r.Category)
            .Include(r => r.Location)
            .Include(r => r.User)
            .Include(r => r.Attachments)
            .Include(r => r.Claims)
            .Include(r => r.LostMatches)
            .Include(r => r.FoundMatches)
            .FirstOrDefaultAsync(r => r.Id == reportId);
    }

    public async Task<bool> UpdateReportAsync(ItemReport report, int userId, bool isAdmin)
    {
        var existing = await _context.ItemReports.FindAsync(report.Id);
        if (existing == null)
            return false;

        if (!isAdmin && existing.UserId != userId)
            return false;

        if (existing.StatusType == enStatusType.Closed && !isAdmin)
            return false;

        existing.ItemName = report.ItemName;
        existing.Color = report.Color;
        existing.ConditionType = report.ConditionType;
        existing.Description = report.Description;
        existing.LocationId = report.LocationId;
        existing.CategoryId = report.CategoryId;
        existing.DateReported = report.DateReported;

        if (isAdmin)
        {
            existing.AdminNotes = report.AdminNotes;
        }

        existing.UpdatedAt = DateTime.UtcNow;

        return true;
    }
    public async Task<bool> DeleteReportAsync(int reportId, int userId, bool isAdmin)
    {
        var report = await _context.ItemReports.FindAsync(reportId);
        if (report == null)
            return false;

        if (!isAdmin && report.UserId != userId)
            return false;

        _context.ItemReports.Remove(report);
        return true;
    }

    public async Task<bool> CancelReportAsync(int reportId, int userId, bool isAdmin)
    {
        var report = await _context.ItemReports.FindAsync(reportId);
        if (report == null)
            return false;

        if (!isAdmin && report.UserId != userId)
            return false;

        if (report.StatusType == enStatusType.Closed ||
            report.StatusType == enStatusType.Returned ||
            report.StatusType == enStatusType.Canceled)
            return false;

        report.StatusType = enStatusType.Canceled;
        report.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public async Task<bool> ChangeStatusAsync(int reportId, enStatusType newStatus, int userId, bool isAdmin)
    {
        var report = await _context.ItemReports.FindAsync(reportId);
        if (report == null)
            return false;

        if (!isAdmin)
            return false;

        report.StatusType = newStatus;
        report.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public async Task<bool> ChangeReportTypeAsync(int reportId, enReportType newType, int userId, bool isAdmin)
    {
        var report = await _context.ItemReports.FindAsync(reportId);
        if (report == null)
            return false;

        if (!isAdmin)
            return false;

        report.ReportType = newType;
        report.UpdatedAt = DateTime.UtcNow;

        return true;
    }

    public async Task<IEnumerable<ItemReport>> GetUserReportsAsync(int userId)
    {
        return await _context.ItemReports
            .Include(r => r.Category)
            .Include(r => r.Location)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.DateReported)
            .ToListAsync();
    }
}