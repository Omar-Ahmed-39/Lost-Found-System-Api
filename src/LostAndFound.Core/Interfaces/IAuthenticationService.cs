using LostAndFound.Core.Entities;

namespace LostAndFound.Core.Interfaces;

public interface IAuthenticationService
{
    Task<(string Token, string RefreshToken)?> LoginAsync(string email, string credential, string? fcmToken);
    Task<bool> RegisterAsync(User user, string password);
    Task<(string Token, string RefreshToken)?> RefreshTokenAsync(string token, string refreshToken);
    Task<bool> RevokeTokenAndLogoutAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}