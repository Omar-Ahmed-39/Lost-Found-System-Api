using LostAndFound.Api.DTOs.Auth;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    /// <summary>Authenticates a user and returns JWT + refresh token.</summary>
    [AllowAnonymous]
    [HttpPost(ApiRoutes.Auth.Login)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password, request.FcmToken);

        if (result is null)
            return Error("Invalid email or password.", StatusCodes.Status401Unauthorized);

        return Success(new AuthResponseDto
        {
            Token = result.Value.Token,
            RefreshToken = result.Value.RefreshToken
        }, "Login successful.");
    }

    /// <summary>Registers a new user account.</summary>
    [AllowAnonymous]
    [HttpPost(ApiRoutes.Auth.Register)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            UserName = request.Email, // Identity requires UserName
            IsActive = true,
            Created = DateTime.UtcNow
        };

        var result = await _authService.RegisterAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Error(errors, StatusCodes.Status400BadRequest);
        }

        return Created<object>(null!, "Account created successfully.");
    }

    /// <summary>Refreshes an expired JWT using a valid refresh token.</summary>
    [AllowAnonymous]
    [HttpPost(ApiRoutes.Auth.Refresh)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto request)
    {
        var result = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);

        if (result is null)
            return Error("Invalid or expired refresh token.", StatusCodes.Status401Unauthorized);

        return Success(new AuthResponseDto
        {
            Token = result.Value.Token,
            RefreshToken = result.Value.RefreshToken
        }, "Token refreshed successfully.");
    }

    /// <summary>Changes the authenticated user's password.</summary>
    [Authorize]
    [HttpPut(ApiRoutes.Auth.ChangePassword)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var success = await _authService.ChangePasswordAsync(GetUserId(), request.CurrentPassword, request.NewPassword);

        if (!success)
            return Error("Password change failed. Current password may be incorrect.");

        return Success<object>(null!, "Password changed successfully.");
    }

    /// <summary>Revokes the current refresh token and logs the user out.</summary>
    [Authorize]
    [HttpPost(ApiRoutes.Auth.Logout)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var success = await _authService.RevokeTokenAndLogoutAsync(GetUserId());

        if (!success)
            return Error("Logout failed. Please try again.");

        return Success<object>(null!, "Logged out successfully.");
    }
}
