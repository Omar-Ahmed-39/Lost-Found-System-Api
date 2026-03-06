using LostAndFound.Infrastructure;

namespace LostAndFound.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(context);
            Matches = new MatchRepository(context);
            ItemReports = new ItemReportRepository(context);
            Locations = new GenericRepository<Location>(context);
            Categories = new GenericRepository<Category>(context);
            Notifications = new NotificationRepository(context);
            Universities = new GenericRepository<University>(context);
            Departments = new GenericRepository<Department>(context);
        }

        public IGenericRepository<Location> Locations { get; }
        public IGenericRepository<Category> Categories { get; }
        public INotificationRepository Notifications { get; }
        public IGenericRepository<University> Universities { get; }
        public IGenericRepository<Department> Departments { get; }
        public IItemReportRepository ItemReports { get; }
        public IMatchRepository Matches { get; }
        public IUserRepository Users { get; }

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
