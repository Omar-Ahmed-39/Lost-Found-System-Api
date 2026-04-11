using Microsoft.AspNetCore.Identity;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;

namespace LostAndFound.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;

            Matches = new MatchRepository(context);
            Claims = new ClaimRepository(context);
            ItemReports = new ItemReportRepository(context);
            Dashboard = new DashboardRepository(context, userManager);

            Locations = new GenericRepository<Location>(context);
            Categories = new GenericRepository<Category>(context);
            Notifications = new NotificationRepository(context);
            Universities = new GenericRepository<University>(context);
            Departments = new GenericRepository<Department>(context);
            Feedbacks = new FeedbackRepository(context);
        }

        public IGenericRepository<Location> Locations { get; }
        public IGenericRepository<Category> Categories { get; }
        public INotificationRepository Notifications { get; }
        public IGenericRepository<University> Universities { get; }
        public IGenericRepository<Department> Departments { get; }

        public IItemReportRepository ItemReports { get; }
        public IMatchRepository Matches { get; }
        public IDashboardRepository Dashboard { get; }
        public IFeedbackRepository Feedbacks { get; }
        public IClaimRepository Claims { get; }

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