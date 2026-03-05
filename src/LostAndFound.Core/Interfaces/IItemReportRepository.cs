using LostAndFound.Core.Entities;
using LostAndFound.Core.Filters;

namespace LostAndFound.Core.Interfaces;

public interface IItemReportRepository : IGenericRepository<ItemReport>
{
    Task<ItemReport> CreateAsync(ItemReport report);
    Task<ItemReport> UpdateAsync(ItemReport Report, int userId, bool isAdmin);
    Task<ItemReport> DeleteAsync(int Report, int userId, bool isAdmin);
    Task<ItemReport> CancelAsync(int Report, int userId, bool isAdmin);
    Task<ItemReport?> GetByIdasync(int reportId);
    Task<IEnumerable<ItemReport>> GetItemReportsAsync();
    Task<IEnumerable<ItemReport>> FilterAsync(ItemReportFilter filter);
}