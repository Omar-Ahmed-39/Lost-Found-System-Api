namespace LostAndFound.Infrastructure.Repository;

public class MatchRepository : GenericRepository<Match>, IMatchRepository
{
    private readonly ApplicationDbContext _context;
    public MatchRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Match>> GetTopMatchesForReportAsync(int reportId, int topN = 5)
    {
        return await _context.Matches
            .AsNoTracking()
            .Where(m => m.LostId == reportId || m.FoundId == reportId)
            .OrderByDescending(m => m.MatchScore)
            .Take(topN)
            .ToListAsync();
    }
}