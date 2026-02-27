namespace LostAndFound.Infrastructure.Repository;

public class ItemReportRepository : GenericRepository<ItemReport>, IItemReportRepository
{
    private readonly ApplicationDbContext _context;
    public ItemReportRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
}