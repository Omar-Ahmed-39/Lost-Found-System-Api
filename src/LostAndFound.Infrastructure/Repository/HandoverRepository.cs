using LostAndFound.Core.Enums;

namespace LostAndFound.Infrastructure.Repository;

public class HandoverRepository : GenericRepository<Handover>, IHandoverRepository
{
    private readonly ApplicationDbContext _context;

    public HandoverRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Handover?> GetDetailsAsync(int id)
    {
        return await _context.Handovers
            .Include(h => h.Location)
            .Include(h => h.ReceiverUser)
            .Include(h => h.HandedByUser)
            .Include(h => h.Claim)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<Handover?> GetByClaimIdAsync(int claimId)
    {
        return await _context.Handovers
            .Include(h => h.Location)
            .Include(h => h.ReceiverUser)
            .Include(h => h.HandedByUser)
            .Include(h => h.Claim)
            .FirstOrDefaultAsync(h => h.ClaimId == claimId);
    }

    public async Task<bool> CreateHandoverAsync(Handover handover, int adminUserId)
    {
        if (handover.LocationId <= 0 || handover.ReceiverUserId <= 0 || handover.ClaimId <= 0)
            return false;

        if (string.IsNullOrWhiteSpace(handover.IdNumber))
            return false;

        var claim = await _context.Claims
            .Include(c => c.Report)
            .FirstOrDefaultAsync(c => c.Id == handover.ClaimId);

        if (claim == null)
            return false;

        if (claim.ApprovalStatus != enApprovalStatus.Approved)
            return false;

        if (claim.Report == null)
            return false;

        if (claim.UserId != handover.ReceiverUserId)
            return false;

        var exists = await _context.Handovers
            .AnyAsync(h => h.ClaimId == handover.ClaimId);

        if (exists)
            return false;

        if (handover.HandoverDate == default)
            handover.HandoverDate = DateTime.UtcNow;

        handover.HandedByUserId = adminUserId;
        handover.CreatedAt = DateTime.UtcNow;
        handover.UpdatedAt = DateTime.UtcNow;

        await _context.Handovers.AddAsync(handover);

        claim.ApprovalStatus = enApprovalStatus.Completed;
        claim.UpdatedAt = DateTime.UtcNow;

        claim.Report.StatusType = enStatusType.Returned;
        claim.Report.UpdatedAt = DateTime.UtcNow;

        return true;
    }
}