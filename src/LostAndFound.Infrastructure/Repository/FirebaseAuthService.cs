using FirebaseAdmin.Auth;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LostAndFound.Infrastructure.Repository;

public class FirebaseAuthService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    private readonly ILogger<FirebaseAuthService> _logger;
    private readonly UserManager<User> _userManager;

    public FirebaseAuthService(IUnitOfWork unitOfWork, IJwtProvider jwtProvider, ILogger<FirebaseAuthService> logger, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<(string Token, string RefreshToken)?> LoginAsync(string email, string credential, string? fcmToken)
    {
        FirebaseToken decodedToken;

        // 1. Verify the Firebase ID token — handle specific failure modes.
        try
        {
            decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(credential);
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.ExpiredIdToken)
        {
            _logger.LogWarning("Firebase token expired for email: {Email}", email);
            throw new UnauthorizedAccessException("The Firebase token has expired. Please re-authenticate.", ex);
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogWarning(ex, "Firebase token validation failed for email: {Email}", email);
            throw new UnauthorizedAccessException("Firebase authentication failed: invalid token.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error communicating with Firebase for email: {Email}", email);
            throw;
        }

        // 2. Cross-check the email claim against the provided email.
        var tokenEmail = decodedToken.Claims.TryGetValue("email", out var emailClaim)
            ? emailClaim?.ToString()
            : null;

        if (string.IsNullOrEmpty(tokenEmail) || !string.Equals(tokenEmail, email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Email in token does not match the provided email.");

        // 3. Sync Firebase user to local DB
        User? user;
        try
        {
            user = await _userManager.FindByEmailAsync(email);
            var shouldSave = false;

            if (user is null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    Name = decodedToken.Claims.TryGetValue("name", out var nameClaim)
                           ? nameClaim?.ToString() ?? string.Empty
                           : string.Empty,
                    IsActive = true,
                    FcmToken = fcmToken,
                    Created = DateTime.UtcNow
                };
                
                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
                shouldSave = false;
            }
            else if (fcmToken != null && user.FcmToken != fcmToken)
            {
                user.FcmToken = fcmToken;
                shouldSave = true;
            }

            if (shouldSave)
            {
                await _userManager.UpdateAsync(user);
            }
        }
        catch (DbUpdateException)
        {
            user = await _userManager.FindByEmailAsync(email)
                   ?? throw new UnauthorizedAccessException("Could not resolve user after concurrent insert conflict.");

            if (fcmToken != null && user.FcmToken != fcmToken)
            {
                user.FcmToken = fcmToken;
                await _userManager.UpdateAsync(user);
            }
        }

        // 4. Guard deactivated users
        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated. Please contact support.");

        // 5. Resolve roles
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
            roles = new List<string> { "User" };

        var token = _jwtProvider.GenerateToken(user, roles);
        return (token, "firebase-refresh-token-managed-by-client");
    }

    public async Task<bool> RevokeTokenAndLogoutAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            user.FcmToken = null;
            await _userManager.UpdateAsync(user);
            return true;
        }
        return false;
    }

    public Task<IdentityResult> RegisterAsync(User user, string password)
    {
        throw new NotSupportedException("Registration is managed by the Firebase client SDK.");
    }

    public Task<(string Token, string RefreshToken)?> RefreshTokenAsync(string token, string refreshToken)
    {
        throw new NotSupportedException("Refresh tokens are handled by the Firebase client SDK.");
    }

    public Task<IdentityResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        throw new NotSupportedException("Password changes are managed by the Firebase client SDK.");
    }
}
