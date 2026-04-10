using LostAndFound.Core.Enums;

namespace LostAndFound.Infrastructure.Repository;

public class HandoverRepository : GenericRepository<Handover>, IHandoverRepository
{
    private readonly ApplicationDbContext _context;

    public HandoverRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> CompleteHandoverAsync(
        int claimId,
        Handover handoverData,
        int adminId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        var claim = await _context.Claims
            .Include(c => c.Report)
            .FirstOrDefaultAsync(c => c.Id == claimId);

        if (claim == null)
            return false;

        if (claim.ApprovalStatus != enApprovalStatus.Approved)
            return false;

        if (claim.Report.StatusType != enStatusType.Open)
            return false;

        // Create Handover
        handoverData.ClaimId = claim.Id;
        handoverData.HandoverDate = DateTime.UtcNow;

        await _context.Handovers.AddAsync(handoverData);

        // Update Claim status to Completed
        claim.ApprovalStatus = enApprovalStatus.Completed;

        // Update Report status to Closed
        claim.Report.StatusType = enStatusType.Closed;

        // Reject other pending claims for the same report
        var otherClaims = await _context.Claims
            .Where(c =>
                c.ReportId == claim.ReportId &&
                c.Id != claim.Id &&
                c.ApprovalStatus == enApprovalStatus.Pending)
            .ToListAsync();

        foreach (var other in otherClaims)
        {
            other.ApprovalStatus = enApprovalStatus.Rejected;
        }

        // Remove any matches related to the report
        await _context.Matches
            .Where(m =>
                m.LostId == claim.ReportId ||
                m.FoundId == claim.ReportId)
            .ExecuteDeleteAsync();

        // ✅ SaveChangesAsync is justified here: this method owns an explicit DB transaction
        //    that spans multiple writes (Handover, Claim, Report, other Claims, Matches).
        //    Callers must NOT call _unitOfWork.SaveAsync() after this — it's already committed.
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }
}
