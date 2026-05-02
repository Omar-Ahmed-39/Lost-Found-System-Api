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

        var response = new FeedbackResponseDto
        {
            Id = feedback.Id,
            Subject = feedback.Subject,
            Message = feedback.Message,
            Rating = feedback.Rating,
            IsReplied = feedback.IsReplied,
            AdminReply = feedback.AdminReply,
            UserId = feedback.UserId,
            UserName = string.Empty, // Will not block serialization, but ideally we don't return entity directly
            UserEmail = string.Empty,
            CreatedAt = feedback.CreatedAt
        };

        return Created(response);
    }

    [HttpGet(ApiRoutes.Feedbacks.GetMyFeedbacks)]
    public async Task<IActionResult> GetMyFeedbacks()
    {
        var userId = GetUserId();
        var feedbacks = await _unitOfWork.Feedbacks.GetUserFeedbacksAsync(userId);

        var response = feedbacks.Select(f => new FeedbackResponseDto
        {
            Id = f.Id,
            Subject = f.Subject,
            Message = f.Message,
            Rating = f.Rating,
            IsReplied = f.IsReplied,
            AdminReply = f.AdminReply,
            UserId = f.UserId,
            UserName = f.User?.Name ?? string.Empty,
            UserEmail = f.User?.Email ?? string.Empty,
            CreatedAt = f.CreatedAt
        });

        return Success(response);
    }

    [HttpGet(ApiRoutes.Feedbacks.GetAllAdmin)]
    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    public async Task<IActionResult> GetAll([FromQuery] bool pendingOnly = false)
    {
        var feedbacks = await _unitOfWork.Feedbacks.GetFeedbacksForAdminAsync(pendingOnly);

        var response = feedbacks.Select(f => new FeedbackResponseDto
        {
            Id = f.Id,
            Subject = f.Subject,
            Message = f.Message,
            Rating = f.Rating,
            IsReplied = f.IsReplied,
            AdminReply = f.AdminReply,
            UserId = f.UserId,
            UserName = f.User?.Name ?? string.Empty,
            UserEmail = f.User?.Email ?? string.Empty,
            CreatedAt = f.CreatedAt
        });

        return Success(response);
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

        return Success(new { message = "Reply added successfully." });
    }
}
