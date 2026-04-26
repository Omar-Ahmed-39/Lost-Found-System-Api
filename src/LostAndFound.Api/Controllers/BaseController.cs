using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace LostAndFound.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;

    /// <summary>
    /// Returns the authenticated user's ID from JWT claims.
    /// Throws <see cref="UnauthorizedAccessException"/> if the claim is missing or invalid.
    /// </summary>
    protected int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("Invalid or missing user identity claim.");

        return userId;
    }

    protected IActionResult Success<T>(T data, string? message = "Operation Completed Successfully.")
    {
        return Ok(ApiResponse<T>.Success(data, message));
    }

    protected IActionResult Created<T>(T data, string? message = "Item Created Successfully.")
    {
        return StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Success(data, message));
    }

    protected IActionResult Error(string error, int statusCode = StatusCodes.Status400BadRequest)
    {
        return StatusCode(statusCode, ApiResponse<object>.Failure(error));
    }

    protected IActionResult Error(List<string> errors, int statusCode = StatusCodes.Status400BadRequest)
    {
        return StatusCode(statusCode, ApiResponse<object>.Failure(errors));
    }

    protected IActionResult Paged<T>(T data, int pageNumber, int pageSize, int totalRecords, string? message = null)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        return Ok(new ApiPagedResponse<T>(data, pageNumber, pageSize, totalRecords, message));
    }

    protected static bool IsValidName(string name)
    {
        return Regex.IsMatch(name, @"^[A-Za-z\s]+$");
    }
}