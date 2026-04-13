using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IHandoverRepository : IGenericRepository<Handover>
{
    Task<Handover?> GetDetailsAsync(int id);
    Task<Handover?> GetByClaimIdAsync(int claimId);
}