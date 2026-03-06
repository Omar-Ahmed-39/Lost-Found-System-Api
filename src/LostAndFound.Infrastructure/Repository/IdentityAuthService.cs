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
    public async Task<string> LoginAsync(string email, string passwordOrFirebaseToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, passwordOrFirebaseToken);
        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);

        return _jwtProvider.GenerateToken(user, roles);
    }

    public Task LogoutAsync()
    {
        // With stateless JWT authentication, logout is typically handled on the client side
        // by removing the token from local storage or cookies.
        // If you implement a token blacklist or refresh tokens in the future,
        // you would handle the revocation logic here.
        return Task.CompletedTask;
    }

    public async Task<bool> RegisterAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return result.Succeeded;
    }
}