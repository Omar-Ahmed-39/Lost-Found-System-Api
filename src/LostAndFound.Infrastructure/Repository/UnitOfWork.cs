using LostAndFound.Infrastructure;

namespace LostAndFound.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context, IUserRepository userRepository,
                IMatchRepository matchRepository, IItemReportRepository itemReportRepository,
                IGenericRepository<Location> locationRepository, IGenericRepository<Category> categoryRepository,
                INotificationRepository notificationRepository, IGenericRepository<University> universityRepository, 
                IGenericRepository<Department> departmentRepository
        )
        {
            _context = context;
            Users = userRepository;
            Matches = matchRepository;
            ItemReports = itemReportRepository;
            Locations = locationRepository;
            Categories = categoryRepository;
            Notifications = notificationRepository;
            Universities = universityRepository;
            Departments = departmentRepository;
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