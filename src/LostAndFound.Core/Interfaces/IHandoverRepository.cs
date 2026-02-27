using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IHandoverRepository : IGenericRepository<Handover>
{
    Task<bool> CompleteHandoverAsync(int claimId, Handover handoverData, int adminId);
}
