using LostAndFound.Core.Entities;
using LostAndFound.Core.Filters;

namespace LostAndFound.Core.Interfaces;

public interface IItemReportRepository : IGenericRepository<ItemReport>
{
    Task<ItemReport> CreateAsync(ItemReport report);
    Task<bool> UpdateAsync(ItemReport report, int userId, bool isAdmin);
    Task<bool> DeleteAsync(int reportId, int userId, bool isAdmin);
    Task<bool> CancelAsync(int reportId, int userId, bool isAdmin);
    Task<ItemReport?> GetByIdAsync(int reportId);
    Task<IEnumerable<ItemReport>> GetItemReportsAsync();
    Task<IEnumerable<ItemReport>> FilterAsync(ItemReportFilter filter);
}