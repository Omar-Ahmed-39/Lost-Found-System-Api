using LostAndFound.Api.DTOs.Claims;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Filters;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers;

public class ClaimsController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public ClaimsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // =========================
    // Admin Endpoints
    // =========================

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet(ApiRoutes.Claims.GetAll)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] enApprovalStatus? approvalStatus,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new ClaimFilter
        {
            Search = search,
            ApprovalStatus = approvalStatus,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _unitOfWork.Claims.GetFilteredAsync(filter, pageNumber, pageSize);

        var response = new List<ClaimResponseDto>();

        foreach (var claim in result.Items)
        {
            var score = await _unitOfWork.Claims.GetMatchScoreForClaimAsync(claim.Id);

            response.Add(new ClaimResponseDto
            {
                Id = claim.Id,
                ClaimCode = $"CLM-{claim.Id:D3}",
                ItemName = claim.Report.ItemName,
                ClaimantName = claim.User.Name,
                ClaimDate = claim.ClaimDate,
                MatchScore = score,
                ApprovalStatus = claim.ApprovalStatus
            });
        }

        return Paged(response, pageNumber, pageSize, result.TotalCount);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet(ApiRoutes.Claims.GetById)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var claim = await _unitOfWork.Claims.GetDetailsAsync(id);
        if (claim == null)
            return Error("Claim not found.", 404);

        var score = await _unitOfWork.Claims.GetMatchScoreForClaimAsync(id);

        var response = new ClaimReviewDetailsDto
        {
            Id = claim.Id,
            ClaimCode = $"CLM-{claim.Id:D3}",
            ClaimDate = claim.ClaimDate,
            ApprovalStatus = claim.ApprovalStatus,
            Remarks = claim.Remarks,
            ItemName = claim.Report.ItemName,
            Description = claim.Report.Description,
            LocationName = claim.Report.Location.Name,
            DateReported = claim.Report.DateReported,
            ClaimantName = claim.User.Name,
            ClaimantEmail = claim.User.Email ?? string.Empty,
            MatchScore = score,
            ItemImages = claim.Report.Attachments.Select(a => a.FilePath).ToList()
        };

        return Success(response);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut(ApiRoutes.Claims.Approve)]
    public async Task<IActionResult> Approve([FromRoute] int id)
    {
        var approved = await _unitOfWork.Claims.ApproveClaimAsync(id, GetUserId());
        if (!approved)
            return Error("Failed to approve claim.", 400);

        await _unitOfWork.SaveAsync();
        return Success(true, "Claim approved successfully.");
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut(ApiRoutes.Claims.Reject)]
    public async Task<IActionResult> Reject([FromRoute] int id, [FromBody] RejectClaimDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Remarks))
            return Error("Remarks are required.");

        var rejected = await _unitOfWork.Claims.RejectClaimAsync(id, dto.Remarks, GetUserId());
        if (!rejected)
            return Error("Failed to reject claim.", 400);

        await _unitOfWork.SaveAsync();
        return Success(true, "Claim rejected successfully.");
    }

    // =========================
    // User / App Endpoints
    // =========================

    [Authorize]
    [HttpPost(ApiRoutes.Claims.Create)]
    public async Task<IActionResult> Create([FromBody] CreateClaimDto dto)
    {
        var created = await _unitOfWork.Claims.CreateClaimAsync(dto.ReportId, GetUserId());
        if (!created)
            return Error("Failed to create claim.", 400);

        await _unitOfWork.SaveAsync();
        return Success(true, "Claim submitted successfully.");
    }
}