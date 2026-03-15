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

    [HttpGet(ApiRoutes.Notifications.GetUserNotifications)]
    public async Task<IActionResult> GetMyNotifications()
    {
        var currentUserId = GetUserId();
        var notifications = await _unitOfWork.Notifications.GetUserNotificationsAsync(currentUserId);

        return Ok(notifications);
    }

    [HttpPut(ApiRoutes.Notifications.MarkAsRead)]
    public async Task<IActionResult> MarkAsRead([FromRoute] int id)
    {
        var isFound = await _unitOfWork.Notifications.MarkAsReadAsync(id, GetUserId());

        if (!isFound)
            return Error("Notification not found or you don't have permission to mark it as read.", 404);

        await _unitOfWork.SaveAsync();
        return Success(true, "Notification marked as read successfully.");
    }
}