using LostAndFound.Core.Entities;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Filters;
using LostAndFound.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Infrastructure.Repository;

public class ClaimRepository : GenericRepository<Claim>, IClaimRepository
{
    private readonly ApplicationDbContext _context;

    public ClaimRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Claim> Items, int TotalCount)> GetFilteredAsync(
        ClaimFilter filter,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<Claim> query = _context.Claims
            .Include(c => c.User)
            .Include(c => c.Report)
                .ThenInclude(r => r.Location)
            .Include(c => c.Report)
                .ThenInclude(r => r.Attachments);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();

            query = query.Where(c =>
                c.User.Name.Contains(search) ||
                c.Report.ItemName.Contains(search) ||
                c.Remarks.Contains(search));
        }

        if (filter.ApprovalStatus.HasValue)
            query = query.Where(c => c.ApprovalStatus == filter.ApprovalStatus.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(c => c.ClaimDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(c => c.ClaimDate <= filter.ToDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.ClaimDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Claim?> GetDetailsAsync(int claimId)
    {
        return await _context.Claims
            .Include(c => c.User)
            .Include(c => c.Report)
                .ThenInclude(r => r.Location)
            .Include(c => c.Report)
                .ThenInclude(r => r.Attachments)
            .Include(c => c.Report)
                .ThenInclude(r => r.User)
            .Include(c => c.Handover)
            .FirstOrDefaultAsync(c => c.Id == claimId);
    }

    public async Task<bool> ApproveClaimAsync(int claimId, int adminUserId)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null)
            return false;

        if (claim.ApprovalStatus != enApprovalStatus.Pending)
            return false;

        claim.ApprovalStatus = enApprovalStatus.Approved;
        claim.UpdatedAt = DateTime.UtcNow;

        var otherClaims = await _context.Claims
            .Where(c => c.ReportId == claim.ReportId && c.Id != claim.Id)
            .ToListAsync();

        foreach (var otherClaim in otherClaims)
        {
            if (otherClaim.ApprovalStatus == enApprovalStatus.Pending)
            {
                otherClaim.ApprovalStatus = enApprovalStatus.Closed;
                otherClaim.UpdatedAt = DateTime.UtcNow;
            }
        }

        return true;
    }

    public async Task<bool> RejectClaimAsync(int claimId, string remarks, int adminUserId)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null) return false;

        if (claim.ApprovalStatus != enApprovalStatus.Pending &&
            claim.ApprovalStatus != enApprovalStatus.Approved)
            return false;

        var normalizedRemarks = remarks.Trim();
        if (string.IsNullOrWhiteSpace(normalizedRemarks))
            return false;

        claim.ApprovalStatus = enApprovalStatus.Rejected;
        claim.Remarks = normalizedRemarks;
        claim.UpdatedAt = DateTime.UtcNow;

        return true;
    }

    public async Task<bool> CancelClaimAsync(int claimId, int userId, bool isAdmin)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null)
            return false;

        if (!isAdmin && claim.UserId != userId)
            return false;

        if (claim.ApprovalStatus != enApprovalStatus.Pending)
            return false;

        claim.ApprovalStatus = enApprovalStatus.Cancelled;
        claim.CancelledAt = DateTime.UtcNow;
        claim.UpdatedAt = DateTime.UtcNow;

        return true;
    }

    public async Task<IEnumerable<Claim>> GetUserClaimsAsync(int userId)
    {
        return await _context.Claims
            .Include(c => c.Report)
            .ThenInclude(r => r.Attachments)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ClaimDate)
            .ToListAsync();
    }

    public async Task<double?> GetMatchScoreForClaimAsync(int claimId)
    {
        var claim = await _context.Claims
            .Include(c => c.Report)
            .FirstOrDefaultAsync(c => c.Id == claimId);

        if (claim == null)
            return null;

        var lostReport = await _context.ItemReports
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.UserId == claim.UserId &&
                r.ReportType == enReportType.Lost &&
                r.CategoryId == claim.Report.CategoryId);

        if (lostReport == null)
            return null;

        var match = await _context.Matches
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.LostId == lostReport.Id &&
                m.FoundId == claim.ReportId);

        return match?.MatchScore;
    }

    public async Task<bool> CreateClaimAsync(int reportId, int userId)
    {
        if (reportId <= 0)
            return false;

        var report = await _context.ItemReports
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
            return false;

        if (report.StatusType == enStatusType.Closed ||
            report.StatusType == enStatusType.Returned ||
            report.StatusType == enStatusType.Canceled)
            return false;

        if (report.UserId == userId)
            return false;

        var exists = await _context.Claims
            .AnyAsync(c => c.ReportId == reportId && c.UserId == userId);

        if (exists)
            return false;

        var claim = new Claim
        {
            ReportId = reportId,
            UserId = userId,
            ClaimDate = DateTime.UtcNow,
            ApprovalStatus = enApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Claims.AddAsync(claim);
        return true;
    }
}