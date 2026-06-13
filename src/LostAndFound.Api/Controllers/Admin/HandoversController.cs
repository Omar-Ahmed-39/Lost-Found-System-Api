using LostAndFound.Api.DTOs.Handovers;
using LostAndFound.Api.Filters;
using LostAndFound.Core.Constants;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers.Admin;

[Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
public class HandoversController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public HandoversController(IUnitOfWork unitOfWork, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    [AuditLog("Created New Handover")]
    [HttpPost(ApiRoutes.Handovers.Create)]
    public async Task<IActionResult> Create([FromForm] CreateHandoverDto dto)
    {
        string imagePath = string.Empty;
        string signaturePath = string.Empty;

        if (dto.IdPhoto != null)
        {
            using var stream = dto.IdPhoto.OpenReadStream();
            imagePath = await _fileService.UploadFileAsync(stream, dto.IdPhoto.FileName, "handovers/ids");
        }

        if (dto.SignatureImage != null)
        {
            using var stream = dto.SignatureImage.OpenReadStream();
            signaturePath = await _fileService.UploadFileAsync(stream, dto.SignatureImage.FileName, "handovers/signatures");
        }

        if (dto.LocationId <= 0 || dto.ReceiverUserId <= 0)
        {
            var claim = await _unitOfWork.Claims.GetDetailsAsync(dto.ClaimId);
            if (claim != null)
            {
                if (dto.LocationId <= 0) dto.LocationId = claim.Report?.LocationId ?? 0;
                if (dto.ReceiverUserId <= 0) dto.ReceiverUserId = claim.UserId;
            }
        }

        var handover = new Handover
        {
            IdType = dto.IdType,
            IdNumber = dto.IdNumber,
            ImagePath = imagePath,
            SignaturePath = signaturePath,
            HandoverDate = dto.HandoverDate,
            Notes = dto.Notes,
            LocationId = dto.LocationId,
            ReceiverUserId = dto.ReceiverUserId,
            ClaimId = dto.ClaimId
        };

        var created = await _unitOfWork.Handovers.CreateHandoverAsync(handover, GetUserId());

        if (!created)
            return Error("Failed to create handover.", 400);

        await _unitOfWork.SaveAsync();

        return Success(true, "Handover created successfully.");
    }

    [HttpGet(ApiRoutes.Handovers.GetById)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var handover = await _unitOfWork.Handovers.GetDetailsAsync(id);
        if (handover == null)
            return Error("Handover not found.", 404);

        var response = new HandoverResponseDto
        {
            Id = handover.Id,
            IdType = handover.IdType,
            IdNumber = handover.IdNumber,
            ImagePath = handover.ImagePath,
            SignaturePath = handover.SignaturePath,
            HandoverDate = handover.HandoverDate,
            Notes = handover.Notes,
            LocationName = handover.Location.Name,
            ReceiverName = handover.ReceiverUser.Name,
            HandedByName = handover.HandedByUser.Name,
            ClaimId = handover.ClaimId
        };

        return Success(response);
    }

    [HttpGet(ApiRoutes.Handovers.GetByClaimId)]
    public async Task<IActionResult> GetByClaimId([FromRoute] int claimId)
    {
        var handover = await _unitOfWork.Handovers.GetByClaimIdAsync(claimId);
        if (handover == null)
            return Error("Handover not found.", 404);

        var response = new HandoverResponseDto
        {
            Id = handover.Id,
            IdType = handover.IdType,
            IdNumber = handover.IdNumber,
            ImagePath = handover.ImagePath,
            SignaturePath = handover.SignaturePath,
            HandoverDate = handover.HandoverDate,
            Notes = handover.Notes,
            LocationName = handover.Location.Name,
            ReceiverName = handover.ReceiverUser.Name,
            HandedByName = handover.HandedByUser.Name,
            ClaimId = handover.ClaimId
        };

        return Success(response);
    }
}