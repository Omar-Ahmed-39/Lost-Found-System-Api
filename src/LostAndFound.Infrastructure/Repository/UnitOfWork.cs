using LostAndFound.Infrastructure;

namespace LostAndFound.Core.Interfaces
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context, IUserRepository userRepository,
                IMatchRepository matchRepository, IItemReportRepository itemReportRepository,
                IGenericRepository<Location> locationRepository, IGenericRepository<Category> categoryRepository,
                IGenericRepository<Notification> notificationRepository
        )
        {
            _context = context;
            Users = userRepository;
            Matches = matchRepository;
            ItemReports = itemReportRepository;
            Locations = locationRepository;
            Categories = categoryRepository;
            Notifications = notificationRepository;
        }

        public IGenericRepository<Location> Locations { get; }
        public IGenericRepository<Category> Categories { get; }
        public IGenericRepository<Notification> Notifications { get; }
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