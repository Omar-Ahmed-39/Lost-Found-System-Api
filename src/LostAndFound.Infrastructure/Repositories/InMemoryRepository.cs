using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using LostAndFound.Core.Interfaces;

namespace LostAndFound.Infrastructure.Repositories
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly ConcurrentDictionary<int, T> _store = new();
        private int _idCounter;

        public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(id, out var value);
            return Task.FromResult(value);
        }

        public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            var id = Interlocked.Increment(ref _idCounter);
            var prop = typeof(T).GetProperty("Id");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(entity, id);
            }
            _store[id] = entity;
            return Task.CompletedTask;
        }
    }
}
