using Microsoft.AspNetCore.Identity;

namespace LostAndFound.Infrastructure.Repository;

public class IdentityAuthService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtProvider _jwtProvider;

    public IdentityAuthService(UserManager<User> userManager, IJwtProvider jwtProvider)
    {
        _userManager = userManager;
        _jwtProvider = jwtProvider;
    }

    public async Task<string> LoginAsync(string email, string credential)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, credential);
        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Guard: deactivated accounts must not receive new tokens.
        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated. Please contact support.");

        var roles = await _userManager.GetRolesAsync(user);
        return _jwtProvider.GenerateToken(user, roles);
    }

    public Task LogoutAsync()
    {
        // Logout is stateless with JWT. To enforce immediate invalidation in the future,
        // implement a token blacklist or refresh-token revocation here.
        return Task.CompletedTask;
    }

    public async Task<bool> RegisterAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return result.Succeeded;
    }
}