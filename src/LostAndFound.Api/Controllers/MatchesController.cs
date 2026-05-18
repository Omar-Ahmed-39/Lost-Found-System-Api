using LostAndFound.Core.Constants;
using LostAndFound.Api.DTOs.Match;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Features.Matches.Commands;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers;

public class MatchesController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public MatchesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // =========================================================================
    // Admin Endpoints  (roles: Admin | SuperAdmin)
    // =========================================================================

    /// <summary>Returns a paginated list of all matches.</summary>
    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    [HttpGet(ApiRoutes.Matches.GetAll)]
    [ProducesResponseType(typeof(ApiPagedResponse<IEnumerable<MatchResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllMatches(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var matches = await _unitOfWork.Matches.GetPagedAsync(pageNumber, pageSize, null, false);
        return Paged(matches.Items.Select(ToDto), pageNumber, pageSize, matches.TotalCount);
    }

    /// <summary>Returns a paginated list of matches with a Pending status.</summary>
    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    [HttpGet(ApiRoutes.Matches.GetPending)]
    [ProducesResponseType(typeof(ApiPagedResponse<IEnumerable<MatchResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendingMatches(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var pending = await _unitOfWork.Matches.GetPagedAsync(
            pageNumber, pageSize,
            m => m.Status == enMatchStatus.Pending,
            false);
        return Paged(pending.Items.Select(ToDto), pageNumber, pageSize, pending.TotalCount);
    }

    /// <summary>Returns a single match by ID (admin view — no ownership check).</summary>
    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
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

    /// <summary>Approves or rejects a pending match. RejectionReason required when IsApproved is false.</summary>
    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    [HttpPut(ApiRoutes.Matches.Verify)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> VerifyMatch(
        [FromRoute] int matchId,
        [FromBody] VerifyMatchDto verifyMatch)
    {
        if (!verifyMatch.IsApproved && string.IsNullOrEmpty(verifyMatch.RejectionReason))
            return Error("Rejection reason is required when rejecting a match.");

        var command = new VerifyMatchCommand(matchId, GetUserId(), verifyMatch.IsApproved, verifyMatch.RejectionReason);
        var isSuccess = await Mediator.Send(command);

        if (!isSuccess)
            return Error("Failed to verify the match. It may not exist or is not in a Pending state.");

        return Success(isSuccess, verifyMatch.IsApproved ? "Match approved successfully." : "Match rejected successfully.");
    }

    // =========================================================================
    // User / App Endpoints  (any authenticated user — IDOR guard enforced)
    // =========================================================================

    /// <summary>
    /// Returns a match by ID.
    /// Only the owner of the lost item associated with the match can call this.
    /// The mobile app calls this after receiving an FCM push that contains the matchId in the data dict.
    /// </summary>
    [Authorize]
    [HttpGet(ApiRoutes.Matches.GetMyMatchById)]
    [ProducesResponseType(typeof(ApiResponse<MatchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyMatchById([FromRoute] int matchId)
    {
        // Load LostItem navigation property — required for the IDOR ownership check.
        var match = await _unitOfWork.Matches.GetAsync(
            m => m.Id == matchId,
            isTracking: false,
            includes: [m => m.LostItem]);

        if (match is null)
            return Error("Match not found.", StatusCodes.Status404NotFound);

        // ✅ IDOR Guard: only the owner of the lost item can view this match.
        if (match.LostItem.UserId != GetUserId())
            return Error("Access denied.", StatusCodes.Status403Forbidden);

        return Success(ToDto(match));
    }

    /// <summary>
    /// Accept a pending match.
    /// Only the owner of the lost item can accept.
    /// </summary>
    [Authorize]
    [HttpPost(ApiRoutes.Matches.Accept)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptMatch([FromRoute] int matchId)
    {
        var match = await _unitOfWork.Matches.GetAsync(
            m => m.Id == matchId,
            isTracking: true,
            includes: [m => m.LostItem]);

        if (match is null)
            return Error("Match not found.", StatusCodes.Status404NotFound);

        // ✅ IDOR Guard
        if (match.LostItem.UserId != GetUserId())
            return Error("Access denied.", StatusCodes.Status403Forbidden);

        if (!match.Accept(GetUserId()))
            return Error("Match is not in a Pending state and cannot be accepted.");

        await _unitOfWork.SaveAsync();
        return Success(true, "Match accepted successfully.");
    }

    /// <summary>
    /// Reject a pending match.
    /// Only the owner of the lost item can reject.
    /// </summary>
    [Authorize]
    [HttpPost(ApiRoutes.Matches.Reject)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectMatch([FromRoute] int matchId, [FromBody] RejectMatchDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            return Error("A rejection reason is required.");

        var match = await _unitOfWork.Matches.GetAsync(
            m => m.Id == matchId,
            isTracking: true,
            includes: [m => m.LostItem]);

        if (match is null)
            return Error("Match not found.", StatusCodes.Status404NotFound);

        // ✅ IDOR Guard
        if (match.LostItem.UserId != GetUserId())
            return Error("Access denied.", StatusCodes.Status403Forbidden);

        if (!match.Reject(GetUserId(), dto.Reason))
            return Error("Match is not in a Pending state or the rejection reason is invalid.");

        await _unitOfWork.SaveAsync();
        return Success(true, "Match rejected successfully.");
    }

    // =========================================================================
    // Shared mapping
    // =========================================================================

    private static MatchResponseDto ToDto(Core.Entities.Match match) => new()
    {
        Id = match.Id,
        MatchScore = match.MatchScore,
        Status = match.Status,
        MatchDate = match.MatchDate,
        CreatedAt = match.CreatedAt,
        UpdatedAt = match.UpdatedAt ?? match.CreatedAt,
        RejectionReason = match.RejectionReason,
        ReviewedBy = match.ReviewedBy,
        ReviewedAt = match.ReviewedAt,
        LostId = match.LostId,
        FoundId = match.FoundId,

    };
}
