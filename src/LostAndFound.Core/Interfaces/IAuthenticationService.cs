namespace LostAndFound.Core.Interfaces;

public interface IAuthenticationService
{
    Task<string> LoginAsync(string email, string passwordOrFirebaseToken);
    Task<bool> RegisterAsync(string email, string password);
    Task LogoutAsync();
}