using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace LostAndFound.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;

            Matches    = new MatchRepository(context);
            Claims     = new ClaimRepository(context);
            ItemReports = new ItemReportRepository(context);
            Dashboard  = new DashboardRepository(context, userManager);

            Users        = new GenericRepository<User>(context);
            Locations    = new GenericRepository<Location>(context);
            Categories   = new GenericRepository<Category>(context);
            Notifications = new NotificationRepository(context);
            Universities = new GenericRepository<University>(context);

            Departments  = new GenericRepository<Department>(context);
            Feedbacks    = new FeedbackRepository(context);
            AuditLogs    = new GenericRepository<AuditLog>(context);
            Handovers    = new HandoverRepository(context);
            ItemAttachments = new GenericRepository<ItemAttachment>(context);
        }

        public IGenericRepository<Location> Locations { get; }
        public IGenericRepository<Category> Categories { get; }
        public IGenericRepository<University> Universities { get; }
        public IGenericRepository<Department> Departments { get; }
        public IGenericRepository<User> Users { get; }
        public INotificationRepository Notifications { get; }

        public IItemReportRepository ItemReports { get; }
        public IMatchRepository Matches { get; }
        public IDashboardRepository Dashboard { get; }
        public IFeedbackRepository Feedbacks { get; }
        public IClaimRepository Claims { get; }
        public IGenericRepository<AuditLog> AuditLogs { get; }
        public IHandoverRepository Handovers { get; }
        public IGenericRepository<ItemAttachment> ItemAttachments { get; }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}