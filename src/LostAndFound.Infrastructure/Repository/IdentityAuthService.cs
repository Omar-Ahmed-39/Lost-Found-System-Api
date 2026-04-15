using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LostAndFound.Infrastructure.Repository;

public class IdentityAuthService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IJwtProvider _jwtProvider;
    private readonly JwtOptions _jwtOptions;

    public IdentityAuthService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IJwtProvider jwtProvider,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtProvider = jwtProvider;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<(string Token, string RefreshToken)?> LoginAsync(string email, string credential, string? fcmToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
            return null;

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, credential);
        if (!isPasswordValid)
            return null;

        if (fcmToken != null)
        {
            user.FcmToken = fcmToken;
            await _userManager.UpdateAsync(user);
        }

        return await GenerateTokensAsync(user);
    }

    public async Task<IdentityResult> RegisterAsync(User user, string password)
    {
        // Internal Identity requirement: ensure UserName is set to Email.
        user.UserName = user.Email;

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded) return result;

        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new Role { Name = "User" });
        }

        await _userManager.AddToRoleAsync(user, "User");
        return IdentityResult.Success;
    }

    public async Task<(string Token, string RefreshToken)?> RefreshTokenAsync(string token, string refreshToken)
    {
        var handler = new JwtSecurityTokenHandler();

        ClaimsPrincipal principal;
        try
        {
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Expired tokens are valid for refresh
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtOptions.SecretKey))
            };

            principal = handler.ValidateToken(token, validationParams, out _);
        }
        catch (SecurityTokenException)
        {
            return null; // Invalid signature — reject the request
        }

        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return null;

        var user = await _userManager.FindByIdAsync(userIdString);
        if (user == null) return null;

        var storedRefreshToken = await _userManager.GetAuthenticationTokenAsync(user, "LostAndFound", "RefreshToken");
        if (storedRefreshToken != refreshToken)
            return null;

        return await GenerateTokensAsync(user);
    }

    public async Task<bool> RevokeTokenAndLogoutAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.FcmToken = null;
        await _userManager.UpdateAsync(user);
        await _userManager.RemoveAuthenticationTokenAsync(user, "LostAndFound", "RefreshToken");

        return true;
    }

    public async Task<IdentityResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    private async Task<(string Token, string RefreshToken)> GenerateTokensAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var tokenString = _jwtProvider.GenerateToken(user, roles);

        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);

        await _userManager.SetAuthenticationTokenAsync(user, "LostAndFound", "RefreshToken", refreshToken);

        return (tokenString, refreshToken);
    }
}