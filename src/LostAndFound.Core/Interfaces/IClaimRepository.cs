using LostAndFound.Core.Entities;
using LostAndFound.Core.Filters;

namespace LostAndFound.Core.Interfaces;

public interface IClaimRepository : IGenericRepository<Claim>
{
    Task<(IEnumerable<Claim> Items, int TotalCount)> GetFilteredAsync(
        ClaimFilter filter,
        int pageNumber,
        int pageSize);

    Task<Claim?> GetDetailsAsync(int claimId);

    Task<bool> ApproveClaimAsync(int claimId, int adminUserId);

    Task<bool> RejectClaimAsync(int claimId, string remarks, int adminUserId);

    Task<bool> CancelClaimAsync(int claimId, int userId, bool isAdmin);

    Task<IEnumerable<Claim>> GetUserClaimsAsync(int userId);

    Task<double?> GetMatchScoreForClaimAsync(int claimId);

    Task<bool> CreateClaimAsync(int reportId, int userId);
}