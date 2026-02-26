using System;
using System.Linq.Expressions;
namespace LostAndFound.Core.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, bool isTracking = false, params Expression<Func<T, object>>[]? includes);
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null, bool isTracking = false, params Expression<Func<T, object>>[]? includes);
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate, bool isTracking = false, params Expression<Func<T, object>>[]? includes);
    Task<T?> FindAsync(object id);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}