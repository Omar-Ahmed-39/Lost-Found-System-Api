using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IMatchRepository : IGenericRepository<Match>
{
    Task<IEnumerable<Match>> GetTopMatchesForReportAsync(int reportId, int topN = 5);
}