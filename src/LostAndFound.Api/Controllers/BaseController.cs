using System.Security.Claims;
using System.Security.Cryptography;
using LostAndFound.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;
    protected int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
    protected IActionResult Success<T>(T data, string? message = "Operation Completed Successfully .")
    {
        return Ok(ApiResponse<T>.Success(data, message));
    }
    protected IActionResult Created<T>(T data, string? message = "Item Created Successfully .")
    {
        return StatusCode(201, ApiResponse<T>.Success(data, message));
    }
    protected IActionResult Error(string error, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResponse<object>.Failure(error));
    }
    protected IActionResult Error(List<string> errors, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResponse<object>.Failure(errors));
    }
    protected IActionResult Paged<T>(T data, int pageNumber, int pageSize, int totalRecords, string? message = null)
    {
        return Ok(new ApiPagedResponse<T>(data, pageNumber, pageSize, totalRecords, message));
    }
}