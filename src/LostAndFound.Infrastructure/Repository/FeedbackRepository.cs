using Microsoft.EntityFrameworkCore;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;

namespace LostAndFound.Infrastructure.Repository;

public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
{
    private readonly ApplicationDbContext _context;

    public FeedbackRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Feedback>> GetFeedbacksForAdminAsync(bool pendingOnly = false)
    {
        var query = _context.Feedbacks
            .Include(f => f.User)
            .AsNoTracking();

        if (pendingOnly)
        {
            query = query.Where(f => !f.IsReplied);
        }

        return await query.OrderByDescending(f => f.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetUserFeedbacksAsync(int userId)
    {
        return await _context.Feedbacks
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }
}
