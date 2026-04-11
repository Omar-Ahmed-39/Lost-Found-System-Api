using LostAndFound.Api.DTOs.Match;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Features.Matches.Commands;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class MatchesController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public MatchesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>Returns a paginated list of all matches.</summary>
    [HttpGet(ApiRoutes.Matches.GetAll)]
    [ProducesResponseType(typeof(ApiPagedResponse<IEnumerable<MatchResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllMatches([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var matches = await _unitOfWork.Matches.GetPagedAsync(pageNumber, pageSize, null, false);
        var dtos = matches.Items.Select(ToDto);
        return Paged(dtos, pageNumber, pageSize, matches.TotalCount);
    }

    /// <summary>Returns a paginated list of matches with a Pending status.</summary>
    [HttpGet(ApiRoutes.Matches.GetPending)]
    [ProducesResponseType(typeof(ApiPagedResponse<IEnumerable<MatchResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendingMatches([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var pendingMatches = await _unitOfWork.Matches.GetPagedAsync(
            pageNumber,
            pageSize,
            m => m.Status == enMatchStatus.Pending,
            false
        );
        var dtos = pendingMatches.Items.Select(ToDto);
        return Paged(dtos, pageNumber, pageSize, pendingMatches.TotalCount);
    }

    /// <summary>Returns a single match by its ID.</summary>
    [HttpGet(ApiRoutes.Matches.GetById)]
    [ProducesResponseType(typeof(ApiResponse<MatchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMatchById([FromRoute] int matchId)
    {
        var match = await _unitOfWork.Matches.GetAsync(m => m.Id == matchId);
        if (match is null)
            return Error("Match not found.", StatusCodes.Status404NotFound);

        return Success(ToDto(match));
    }

    /// <summary>Approves or rejects a pending match. RejectionReason is required when IsApproved is false.</summary>
    [HttpPut(ApiRoutes.Matches.Verify)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> VerifyMatch([FromRoute] int matchId, [FromBody] VerifyMatchDto verifyMatch)
    {
        if (!verifyMatch.IsApproved && string.IsNullOrEmpty(verifyMatch.RejectionReason))
            return Error("Rejection reason is required when rejecting a match.");

        var command = new VerifyMatchCommand(matchId, GetUserId(), verifyMatch.IsApproved, verifyMatch.RejectionReason);
        var isSuccess = await Mediator.Send(command);

        if (!isSuccess)
            return Error("Failed to verify the match. The match may not exist or is not in a Pending state.");

        return Success(isSuccess, verifyMatch.IsApproved ? "Match approved successfully." : "Match rejected successfully.");
    }

    private static MatchResponseDto ToDto(Core.Entities.Match match) => new()
    {
        Id = match.Id,
        MatchScore = match.MatchScore,
        Status = match.Status,
        MatchDate = match.MatchDate,
        CreatedAt = match.CreatedAt,
        UpdatedAt = match.UpdatedAt,
        RejectionReason = match.RejectionReason,
        ReviewedBy = match.ReviewedBy,
        ReviewedAt = match.ReviewedAt,
        LostId = match.LostId,
        FoundId = match.FoundId,
        MatchedBy = match.MatchedBy
    };
}