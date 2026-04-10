using LostAndFound.Api.DTOs.Notifications;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers;

[Authorize]
public class NotificationsController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>Returns all notifications for the authenticated user.</summary>
    [HttpGet(ApiRoutes.Notifications.GetUserNotifications)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyNotifications()
    {
        var currentUserId = GetUserId();
        var notifications = await _unitOfWork.Notifications.GetUserNotificationsAsync(currentUserId);

        var dtos = notifications.Select(n => new NotificationResponseDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });

        return Success(dtos);
    }

    /// <summary>Marks a specific notification as read. Only the owning user can perform this action.</summary>
    [HttpPut(ApiRoutes.Notifications.MarkAsRead)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead([FromRoute] int id)
    {
        var isFound = await _unitOfWork.Notifications.MarkAsReadAsync(id, GetUserId());

        if (!isFound)
            return Error("Notification not found or you don't have permission to mark it as read.", StatusCodes.Status404NotFound);

        await _unitOfWork.SaveAsync();
        return Success(true, "Notification marked as read successfully.");
    }
}