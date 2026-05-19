using LostAndFound.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
namespace LostAndFound.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Location> Locations { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<University> Universities { get; }
    IGenericRepository<Department> Departments { get; }
    IGenericRepository<User> Users { get; }
    INotificationRepository Notifications { get; }
    IItemReportRepository ItemReports { get; }
    IMatchRepository Matches { get; }
    IDashboardRepository Dashboard { get; }
    IFeedbackRepository Feedbacks { get; }
    IClaimRepository Claims { get; }
    IGenericRepository<AuditLog> AuditLogs { get; }
    IHandoverRepository Handovers { get; }
    IGenericRepository<ItemAttachment> ItemAttachments { get; }
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<int> SaveAsync();
}
