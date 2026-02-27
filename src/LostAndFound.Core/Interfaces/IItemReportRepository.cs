using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IItemReportRepository : IGenericRepository<ItemReport>
{
    Task<bool> UpdateReportAsync(ItemReport report, int userId, bool isAdmin);
    Task<bool> DeleteReportAsync(int reportId, int userId, bool isAdmin);
    Task<bool> CloseReportAsync(int reportId);
}