using LostAndFound.Core.Entities;
namespace LostAndFound.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Location> Locations { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<University> Universities { get; }
    IGenericRepository<Department> Departments { get; }
    INotificationRepository Notifications { get; }
    IItemReportRepository ItemReports { get; }
    IMatchRepository Matches { get; }
    IUserRepository Users { get; }
    Task<int> SaveAsync();
}
