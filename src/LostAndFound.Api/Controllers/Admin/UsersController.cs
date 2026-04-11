using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LostAndFound.Core.Entities;
using LostAndFound.Api.DTOs.Users;
using LostAndFound.Api.Controllers;

namespace LostAndFound.Api.Controllers.Admin;

[ApiController]
public class UsersController : BaseController
{
    private readonly UserManager<User> _userManager;

    public UsersController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet(ApiRoutes.Users.GetAll)]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = _userManager.Users.AsNoTracking();
        var totalRecords = await query.CountAsync();
        
        var users = await query
            .OrderByDescending(u => u.Created)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responseList = new List<UserResponseDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            responseList.Add(MapToResponse(user, roles));
        }

        return Paged((IEnumerable<UserResponseDto>)responseList, pageNumber, pageSize, totalRecords);
    }

    [HttpGet(ApiRoutes.Users.GetById)]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(ApiResponse<object>.Failure($"User with ID {id} not found."));

        var roles = await _userManager.GetRolesAsync(user);
        return Success(MapToResponse(user, roles));
    }

    [HttpPost(ApiRoutes.Users.Create)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var user = new User 
        { 
            UserName = dto.Email, 
            Email = dto.Email,
            Name = dto.Name,
            Created = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return Error(result.Errors.Select(e => e.Description).ToList());

        // By default assign "User" role
        await _userManager.AddToRoleAsync(user, "User");
        var roles = await _userManager.GetRolesAsync(user);

        return Created(MapToResponse(user, roles), "User created successfully.");
    }

    [HttpPut(ApiRoutes.Users.Update)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(ApiResponse<object>.Failure($"User with ID {id} not found."));

        user.Name = dto.Name;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Error(result.Errors.Select(e => e.Description).ToList());

        var roles = await _userManager.GetRolesAsync(user);
        return Success(MapToResponse(user, roles), "User updated successfully.");
    }

    [HttpDelete(ApiRoutes.Users.Delete)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(ApiResponse<object>.Failure($"User with ID {id} not found."));

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return Error(result.Errors.Select(e => e.Description).ToList());

        return Success((object)null!, "User deleted successfully.");
    }

    [HttpPatch(ApiRoutes.Users.ToggleBlock)]
    public async Task<IActionResult> ToggleBlockUser(int id, [FromQuery] bool block = true)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(ApiResponse<object>.Failure($"User with ID {id} not found."));

        user.IsActive = !block;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Error(result.Errors.Select(e => e.Description).ToList());

        var roles = await _userManager.GetRolesAsync(user);
        var status = block ? "blocked" : "unblocked";
        return Success(MapToResponse(user, roles), $"User has been {status} successfully.");
    }

    [HttpPatch(ApiRoutes.Users.ChangeRole)]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeUserRoleDto dto)
    {
        var targetRole = dto.Role.Trim();
        if (!targetRole.Equals("User", StringComparison.OrdinalIgnoreCase) && 
            !targetRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Error("Role must be strictly 'User' or 'Admin'.");
        }

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(ApiResponse<object>.Failure($"User with ID {id} not found."));

        var currentRoles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        
        if (!result.Succeeded)
            return Error(result.Errors.Select(e => e.Description).ToList());

        result = await _userManager.AddToRoleAsync(user, targetRole);
        if (!result.Succeeded)
            return Error(result.Errors.Select(e => e.Description).ToList());

        return Success(MapToResponse(user, new List<string> { targetRole }), "User role updated successfully.");
    }

    private UserResponseDto MapToResponse(User user, IList<string> roles)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            Created = user.Created,
            Roles = roles
        };
    }
}
