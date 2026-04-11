using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IFeedbackRepository : IGenericRepository<Feedback>
{
    Task<IEnumerable<Feedback>> GetFeedbacksForAdminAsync(bool pendingOnly = false);
    Task<IEnumerable<Feedback>> GetUserFeedbacksAsync(int userId);
}
