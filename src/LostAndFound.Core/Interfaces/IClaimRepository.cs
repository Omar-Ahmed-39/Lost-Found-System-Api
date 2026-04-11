using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IClaimRepository : IGenericRepository<Claim>
{
    Task<bool> ApproveClaimAsync(int claimId, int adminId);
    Task<bool> RejectClaimAsync(int claimId, int adminId, string remarks);
    Task<bool> CancelClaimAsync(int claimId, int userId);
}
