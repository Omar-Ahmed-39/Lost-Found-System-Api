using System.Linq.Expressions;

namespace LostAndFound.Infrastructure.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

        await _context.Set<T>().AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        if (entities is null)
            throw new ArgumentNullException(nameof(entities), "Entities cannot be null.");

        await _context.Set<T>().AddRangeAsync(entities);
    }

    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate is null)
            return _context.Set<T>().CountAsync();

        return _context.Set<T>().CountAsync(predicate);
    }

    public async Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        var rowAffected = await _context.Set<T>()
            .Where(predicate)
            .ExecuteDeleteAsync();

        return rowAffected > 0;
    }

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().AnyAsync(predicate);
    }

    public async Task<T?> FindAsync(object id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, bool isTracking = false, params Expression<Func<T, object>>[]? includes)
    {
        IQueryable<T> query = _context.Set<T>();

        if (!isTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var navigationProperty in includes)
            {
                query = query.Include(navigationProperty);
            }
        }

        if (predicate != null)
            query = query.Where(predicate);

        return await query.ToListAsync();
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, bool isTracking = false, params Expression<Func<T, object>>[]? includes)
    {
        IQueryable<T> query = _context.Set<T>();

        if (!isTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var navigationProperty in includes)
            {
                query = query.Include(navigationProperty);
            }
        }

        if (predicate != null)
            query = query.Where(predicate);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null, bool isTracking = false, params Expression<Func<T, object>>[]? includes)
    {
        IQueryable<T> query = _context.Set<T>();

        if (!isTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var navigationProperty in includes)
            {
                query = query.Include(navigationProperty);
            }
        }

        if (predicate != null)
            query = query.Where(predicate);

        // ✅ Count first on the full filtered set, then apply pagination
        //    This avoids re-executing the same predicate/includes a second time on the paged slice
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public void Remove(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

        _context.Set<T>().Remove(entity);
    }

    public void Update(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

        _context.Set<T>().Update(entity);
    }
}