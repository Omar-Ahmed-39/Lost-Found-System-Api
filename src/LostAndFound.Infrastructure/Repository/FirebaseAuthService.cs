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

    public async Task<string> LoginAsync(string email, string credential)
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
            if (user is null)
            {
                user = new User
                {
                    Email = email,
                    Name = decodedToken.Claims.TryGetValue("name", out var nameClaim)
                           ? nameClaim?.ToString() ?? string.Empty
                           : string.Empty,
                    IsActive = true
                };
                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveAsync();
            }
        }
        catch (DbUpdateException)
        {
            // Two concurrent first-logins raced — the duplicate insert lost. Re-fetch the winner's record.
            _logger.LogWarning("Race condition on first-login sync for email: {Email}. Re-fetching user.", email);
            user = await _unitOfWork.Users.GetAsync(u => u.Email == email, true)
                   ?? throw new UnauthorizedAccessException("Could not resolve user after concurrent insert conflict.");
        }

        // 4. Guard: deactivated users must not receive new tokens, even with a valid Firebase token.
        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated. Please contact support.");

        // 5. Resolve real roles from local DB; fall back to "User" for brand-new accounts.
        var roles = await _unitOfWork.Users.GetRolesAsync(user.Id);
        if (roles.Count == 0)
            roles = new List<string> { "User" };

        return _jwtProvider.GenerateToken(user, roles);
    }

    public Task LogoutAsync()
    {
        // Logout for Firebase-authenticated users is handled on the client side
        // by calling FirebaseAuth.signOut() in the client SDK.
        // If server-side token revocation is needed in the future, call
        // FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(uid) here.
        return Task.CompletedTask;
    }

    public Task<bool> RegisterAsync(User user, string password)
    {
        // Firebase manages registration on the client side via the Firebase client SDK.
        // Pre-provisioning a DB record is not needed here — it happens automatically
        // on the user's first successful LoginAsync call (the sync block above).
        throw new NotSupportedException(
            "Registration is managed by the Firebase client SDK. " +
            "The local user record is created automatically on the first LoginAsync call.");
    }
}