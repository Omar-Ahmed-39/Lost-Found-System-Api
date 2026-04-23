using LostAndFound.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LostAndFound.Core.Interfaces;
using LostAndFound.Core.Entities;
using LostAndFound.Api.DTOs.Feedback;

namespace LostAndFound.Api.Controllers;

[ApiController]
[Authorize]
public class FeedbackController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public FeedbackController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost(ApiRoutes.Feedbacks.Create)]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackDto dto)
    {
        var userId = GetUserId();

        var feedback = new Feedback
        {
            UserId = userId,
            Subject = dto.Subject,
            Message = dto.Message,
            Rating = dto.Rating,
            IsReplied = false
            // CreatedAt is handled by the Interceptor
        };

        await _unitOfWork.Feedbacks.AddAsync(feedback);
        await _unitOfWork.SaveAsync();

        return Created(feedback);
    }

    [HttpGet(ApiRoutes.Feedbacks.GetMyFeedbacks)]
    public async Task<IActionResult> GetMyFeedbacks()
    {
        var userId = GetUserId();
        var feedbacks = await _unitOfWork.Feedbacks.GetUserFeedbacksAsync(userId);

        return Success(feedbacks);
    }

    [HttpGet(ApiRoutes.Feedbacks.GetAllAdmin)]
    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    public async Task<IActionResult> GetAll([FromQuery] bool pendingOnly = false)
    {
        var feedbacks = await _unitOfWork.Feedbacks.GetFeedbacksForAdminAsync(pendingOnly);
        return Success(feedbacks);
    }

    [HttpPost(ApiRoutes.Feedbacks.Reply)]
    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyFeedbackDto dto)
    {
        var feedback = await _unitOfWork.Feedbacks.FindAsync(id);

        if (feedback == null)
            return Error("Feedback not found.", StatusCodes.Status404NotFound);

        feedback.AdminReply = dto.AdminReply;
        feedback.IsReplied = true;

        _unitOfWork.Feedbacks.Update(feedback);
        await _unitOfWork.SaveAsync();

        return Success(feedback);
    }
}
