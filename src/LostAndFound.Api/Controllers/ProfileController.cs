using LostAndFound.Api;
using LostAndFound.Api.Controllers;
using LostAndFound.Api.DTOs.Profile;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers;

[ApiController]
[Authorize]
public class ProfileController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IFileService _fileService;

    public ProfileController(UserManager<User> userManager, IFileService fileService)
    {
        _userManager = userManager;
        _fileService = fileService;
    }

    [HttpGet(ApiRoutes.Profile.GetMyProfile)]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            return Error("User not found.", StatusCodes.Status404NotFound);

        var profile = new ProfileResponseDto
        {
            Name = user.Name,
            Email = user.Email!,
            avatarUrl = user.avatarUrl
        };

        return Success(profile);
    }

    [HttpPut(ApiRoutes.Profile.UpdateMyProfile)]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto request)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            return Error("User not found.", StatusCodes.Status404NotFound);

        user.Name = request.Name;
        user.Email = request.Email;
        user.UserName = request.Email;

        if (request.ProfileImage != null)
        {
            using var stream = request.ProfileImage.OpenReadStream();
            if (!string.IsNullOrEmpty(user.avatarUrl))
            {
                user.avatarUrl = await _fileService.UpdateFileAsync(stream, request.ProfileImage.FileName, user.avatarUrl, "ProfileImages");
            }
            else
            {
                user.avatarUrl = await _fileService.UploadFileAsync(stream, request.ProfileImage.FileName, "ProfileImages");
            }
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Error(result.Errors.Select(e => e.Description).ToList());

        var profile = new ProfileResponseDto
        {
            Name = user.Name,
            Email = user.Email,
            avatarUrl = user.avatarUrl
        };

        return Success(profile, "Profile updated successfully.");
    }
}