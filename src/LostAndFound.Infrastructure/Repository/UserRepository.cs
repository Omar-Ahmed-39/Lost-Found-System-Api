namespace LostAndFound.Infrastructure.Repository;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private ApplicationDbContext _context;
    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking() 
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<IList<string>> GetRolesAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.Roles.Select(r => r.Name ?? string.Empty).ToList()
               ?? new List<string>();
    }
}