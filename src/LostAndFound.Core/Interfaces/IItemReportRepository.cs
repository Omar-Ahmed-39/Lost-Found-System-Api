using LostAndFound.Core.Entities;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Filters;

namespace LostAndFound.Core.Interfaces;

public interface IItemReportRepository : IGenericRepository<ItemReport>
{
    Task<(IEnumerable<ItemReport> Items, int TotalCount)> GetFilteredAsync(
        ItemReportFilter filter,
        int pageNumber,
        int pageSize);

    Task<ItemReport?> GetDetailsAsync(int reportId);

    Task<bool> UpdateReportAsync(ItemReport report, int userId, bool isAdmin);

    Task<bool> DeleteReportAsync(int reportId, int userId, bool isAdmin);

    Task<bool> CancelReportAsync(int reportId, int userId, bool isAdmin);

    Task<bool> ChangeStatusAsync(int reportId, enStatusType newStatus, int userId, bool isAdmin);

    Task<bool> ChangeReportTypeAsync(int reportId, enReportType newType, int userId, bool isAdmin);

    Task<IEnumerable<ItemReport>> GetUserReportsAsync(int userId);
}