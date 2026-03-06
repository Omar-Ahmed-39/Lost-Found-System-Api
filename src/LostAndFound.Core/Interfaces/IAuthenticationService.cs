using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IAuthenticationService
{
    Task<string> LoginAsync(string email, string credential);
    Task<bool> RegisterAsync(User user, string password);
    Task LogoutAsync();
}