using LostAndFound.Core.Entities;
namespace LostAndFound.Core.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<Location> Location { get; }
    IGenericRepository<Category> Category { get; }
    IUserRepository User { get; }
    Task<int> SaveAsync();
}