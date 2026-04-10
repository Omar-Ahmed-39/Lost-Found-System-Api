using LostAndFound.Core.Enums;

namespace LostAndFound.Infrastructure.Repository;

public class ClaimRepository : GenericRepository<Claim>, IClaimRepository
{
    private readonly ApplicationDbContext _context;

    public ClaimRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ApproveClaimAsync(int claimId, int adminId)
    {
        var claim = await _context.Claims
            .Include(c => c.Report)
            .FirstOrDefaultAsync(c => c.Id == claimId);

        if (claim == null)
            return false;

        if (claim.ApprovalStatus != enApprovalStatus.Pending)
            return false;

        if (claim.Report.ReportType != enReportType.Found)
            return false;

        if (claim.Report.StatusType != enStatusType.Open)
            return false;

        // Ensure only one claim can be approved per report
        var approvedExists = await _context.Claims
            .AnyAsync(c =>
                c.ReportId == claim.ReportId &&
                c.ApprovalStatus == enApprovalStatus.Approved);

        if (approvedExists)
            return false;

        claim.ApprovalStatus = enApprovalStatus.Approved;
        claim.UpdatedAt = DateTime.UtcNow;

        // ✅ SaveChangesAsync removed — caller owns the transaction boundary
        return true;
    }

    public async Task<bool> RejectClaimAsync(int claimId, int adminId, string remarks)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null)
            return false;

        if (claim.ApprovalStatus == enApprovalStatus.Completed)
            return false;

        claim.ApprovalStatus = enApprovalStatus.Rejected;
        claim.Remarks = remarks;
        claim.UpdatedAt = DateTime.UtcNow;

        // ✅ SaveChangesAsync removed — caller owns the transaction boundary
        return true;
    }

    public async Task<bool> CancelClaimAsync(int claimId, int userId)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null)
            return false;

        if (claim.UserId != userId)
            return false;

        if (claim.ApprovalStatus != enApprovalStatus.Pending)
            return false;

        claim.ApprovalStatus = enApprovalStatus.Cancelled;
        claim.CancelledAt = DateTime.UtcNow;

        // ✅ SaveChangesAsync removed — caller owns the transaction boundary
        return true;
    }
}
