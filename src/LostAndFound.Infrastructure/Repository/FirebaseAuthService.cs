using FirebaseAdmin.Auth;
using LostAndFound.Core.Interfaces;

namespace LostAndFound.Infrastructure.Repository;

public class FirebaseAuthService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    public FirebaseAuthService(IUnitOfWork unitOfWork, IJwtProvider jwtProvider)
    {
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
    }
    public async Task<string> LoginAsync(string email, string passwordOrFirebaseToken)
    {
        try
        {
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(passwordOrFirebaseToken);

            string tokenEmail = decodedToken.Claims["email"]?.ToString();
            if (string.IsNullOrEmpty(tokenEmail) || tokenEmail != email)
            {
                throw new UnauthorizedAccessException("Email in token does not match the provided email.");
            }

            var user = await _unitOfWork.Users.GetAsync(u => u.Email == email, true);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    Name = decodedToken.Claims["name"]?.ToString() ?? string.Empty,
                    IsActive = true
                };
                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveAsync();
            }
            return _jwtProvider.GenerateToken(user, new List<string> { "User" });
        }
        catch (Exception ex)
        {
            // Log the exception (ex) here using your logging framework
            throw new UnauthorizedAccessException("Firebase authentication failed.", ex);
        }

    }

    public Task LogoutAsync()
    {
        // With Firebase token authentication, logout is typically handled on the client side
        // by signing out via the Firebase client SDK.
        return Task.CompletedTask;
    }

    public async Task<bool> RegisterAsync(User user, string password)
    {
        return await Task.FromResult(true);
    }
}