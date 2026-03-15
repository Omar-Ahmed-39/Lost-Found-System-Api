using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LostAndFound.Infrastructure.Repository;

public class IdentityAuthService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IConfiguration _configuration;

    public IdentityAuthService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
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

    public async Task<bool> RegisterAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded) return false;

        if (!await _roleManager.RoleExistsAsync("Student"))
        {
            await _roleManager.CreateAsync(new Role { Name = "Student" });
        }

        await _userManager.AddToRoleAsync(user, "Student");
        return true;
    }

    public async Task<(string Token, string RefreshToken)?> RefreshTokenAsync(string token, string refreshToken)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token)) return null;

        var jwtToken = handler.ReadJwtToken(token);
        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (email == null) return null;

        var user = await _userManager.FindByEmailAsync(email);
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

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    private async Task<(string Token, string RefreshToken)> GenerateTokensAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var authClaims = new List<System.Security.Claims.Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            authClaims.Add(new(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            expires: DateTime.Now.AddHours(2),
            claims: authClaims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);

        await _userManager.SetAuthenticationTokenAsync(user, "LostAndFound", "RefreshToken", refreshToken);

        return (tokenString, refreshToken);
    }
}