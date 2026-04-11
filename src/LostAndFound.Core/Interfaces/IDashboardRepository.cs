using LostAndFound.Core.Domain.Dashboard;

namespace LostAndFound.Core.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardData> GetDashboardDataAsync();
}