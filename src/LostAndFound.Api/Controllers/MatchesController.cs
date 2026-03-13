using LostAndFound.Api.DTOs.Match;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Features.Matches.Commands;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
namespace LostAndFound.Api.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class MatchesController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;
    public MatchesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet(ApiRoutes.Matches.GetAll)]
    public async Task<IActionResult> GetAllMatches([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var matches = await _unitOfWork.Matches.GetPagedAsync(
            pageNumber,
            pageSize,
            null,
            false
        );
        return Paged(matches.Items, pageNumber, pageSize, matches.TotalCount);
    }

    [HttpGet(ApiRoutes.Matches.GetPending)]
    public async Task<IActionResult> GetPendingMatches([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var pendingMatches = await _unitOfWork.Matches.GetPagedAsync(
            pageNumber,
            pageSize,
            m => m.Status == enMatchStatus.Pending,
            false
        );
        return Paged(pendingMatches.Items, pageNumber, pageSize, pendingMatches.TotalCount);
    }

    [HttpGet(ApiRoutes.Matches.GetById)]
    public async Task<IActionResult> GetMatchById([FromRoute] int matchId)
    {
        var match = await _unitOfWork.Matches.GetAsync(m => m.Id == matchId);
        if (match == null)
            return Error("Match not found.", 404);

        return Success(match);
    }

    [HttpPut(ApiRoutes.Matches.Verify)]
    public async Task<IActionResult> VerifyMatch([FromRoute] int matchId, [FromBody] VerifyMatchDto verifyMatch)
    {
        if (!verifyMatch.IsApproved && string.IsNullOrEmpty(verifyMatch.RejectionReason))
        {
            return Error("Rejection reason is required when rejecting a match.");
        }

        var command = new VerifyMatchCommand(
            matchId,
            GetUserId(),
            verifyMatch.IsApproved,
            verifyMatch.RejectionReason
        );

        var isSuccess = await Mediator.Send(command);
        if (!isSuccess)
        {
            return Error("Failed to verify the match. Please try again.", 400);
        }

        return Success(isSuccess, verifyMatch.IsApproved ? "Match approved successfully." : "Match rejected successfully.");
    }

}