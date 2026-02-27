using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsEmailUniqueAsync(string email);
}