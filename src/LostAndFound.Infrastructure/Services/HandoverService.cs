using LostAndFound.Core.Entities;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Interfaces;

namespace LostAndFound.Core.Domain;

public class HandoverService : IHandoverService
{
    private readonly IUnitOfWork _unitOfWork;

    public HandoverService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CreateHandoverAsync(Handover handover, int adminUserId)
    {
        var claim = await _unitOfWork.Claims.GetDetailsAsync(handover.ClaimId);
        if (claim == null)
            return false;

        if (claim.ApprovalStatus != enApprovalStatus.Approved)
            return false;

        if (claim.Report == null)
            return false;

        var existingHandover = await _unitOfWork.Handovers.GetByClaimIdAsync(handover.ClaimId);
        if (existingHandover != null)
            return false;

        if (handover.HandoverDate == default)
            handover.HandoverDate = DateTime.UtcNow;

        handover.HandedByUserId = adminUserId;
        handover.CreatedAt = DateTime.UtcNow;
        handover.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Handovers.AddAsync(handover);

        claim.ApprovalStatus = enApprovalStatus.Completed;
        claim.UpdatedAt = DateTime.UtcNow;

        claim.Report.StatusType = enStatusType.Returned;
        claim.Report.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();
        return true;
    }
}