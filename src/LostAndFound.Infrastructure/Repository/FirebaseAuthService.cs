using FirebaseAdmin.Auth;
using LostAndFound.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LostAndFound.Infrastructure.Repository;

public class FirebaseAuthService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    private readonly ILogger<FirebaseAuthService> _logger;

    public FirebaseAuthService(IUnitOfWork unitOfWork, IJwtProvider jwtProvider, ILogger<FirebaseAuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
        _logger = logger;
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
            // Transient / network errors — do NOT convert to UnauthorizedAccess, let global middleware return 503.
            _logger.LogError(ex, "Unexpected error communicating with Firebase for email: {Email}", email);
            throw;
        }

        // 2. Cross-check the email claim against the provided email to prevent token substitution attacks.
        var tokenEmail = decodedToken.Claims.TryGetValue("email", out var emailClaim)
            ? emailClaim?.ToString()
            : null;

        if (string.IsNullOrEmpty(tokenEmail) || !string.Equals(tokenEmail, email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Email in token does not match the provided email.");

        // 3. Sync Firebase user to local DB (idempotent, with race-condition guard).
        User? user;
        try
        {
            user = await _unitOfWork.Users.GetAsync(u => u.Email == email, true);
            var shouldSave = false;

            if (user is null)
            {
                user = new User
                {
                    Email = email,
                    Name = decodedToken.Claims.TryGetValue("name", out var nameClaim)
                           ? nameClaim?.ToString() ?? string.Empty
                           : string.Empty,
                    IsActive = true,
                    FcmToken = fcmToken
                };
                await _unitOfWork.Users.AddAsync(user);
                shouldSave = true;
            }
            else if (fcmToken != null && user.FcmToken != fcmToken)
            {
                user.FcmToken = fcmToken;
                shouldSave = true;
            }

            if (shouldSave)
            {
                await _unitOfWork.SaveAsync();
            }
        }
        catch (DbUpdateException)
        {
            // Two concurrent first-logins raced — the duplicate insert lost. Re-fetch the winner's record.
            _logger.LogWarning("Race condition on first-login sync for email: {Email}. Re-fetching user.", email);
            user = await _unitOfWork.Users.GetAsync(u => u.Email == email, true)
                   ?? throw new UnauthorizedAccessException("Could not resolve user after concurrent insert conflict.");

            if (fcmToken != null && user.FcmToken != fcmToken)
            {
                user.FcmToken = fcmToken;
                await _unitOfWork.SaveAsync();
            }
        }

        // 4. Guard: deactivated users must not receive new tokens, even with a valid Firebase token.
        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated. Please contact support.");

        // 5. Resolve real roles from local DB; fall back to "User" for brand-new accounts.
        var roles = await _unitOfWork.Users.GetRolesAsync(user.Id);
        if (roles.Count == 0)
            roles = new List<string> { "User" };

        var token = _jwtProvider.GenerateToken(user, roles);
        return (token, "firebase-refresh-token-managed-by-client");
    }

    public async Task<bool> RevokeTokenAndLogoutAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId, true);
        if (user != null)
        {
            user.FcmToken = null;
            await _unitOfWork.SaveAsync();
            return true;
        }
        return false;
    }

    public Task<bool> RegisterAsync(User user, string password)
    {
        // Firebase manages registration on the client side via the Firebase client SDK.
        throw new NotSupportedException(
            "Registration is managed by the Firebase client SDK. " +
            "The local user record is created automatically on the first LoginAsync call.");
    }

    public Task<(string Token, string RefreshToken)?> RefreshTokenAsync(string token, string refreshToken)
    {
        throw new NotSupportedException("Refresh tokens are handled by the Firebase client SDK.");
    }

    public Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        throw new NotSupportedException("Password changes are managed by the Firebase client SDK.");
    }
}