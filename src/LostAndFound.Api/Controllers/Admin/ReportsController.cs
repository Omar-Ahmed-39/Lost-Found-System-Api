using LostAndFound.Api.DTOs.ItemReports;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Filters;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers.Admin;

[Authorize(Roles = "Admin,SuperAdmin")]
public class ReportsController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet(ApiRoutes.Reports.GetAll)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? locationId,
        [FromQuery] enStatusType? statusType,
        [FromQuery] enReportType? reportType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new ItemReportFilter
        {
            Search = search,
            CategoryId = categoryId,
            LocationId = locationId,
            StatusType = statusType,
            ReportType = reportType,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _unitOfWork.ItemReports.GetFilteredAsync(filter, pageNumber, pageSize);

        var response = result.Items.Select(r => new ItemReportResponseDto
        {
            Id = r.Id,
            ItemName = r.ItemName,
            Color = r.Color,
            ConditionType = r.ConditionType,
            StatusType = r.StatusType,
            ReportType = r.ReportType,
            DateReported = r.DateReported,
            CategoryName = r.Category.Name,
            LocationName = r.Location.Name,
            UserName = r.User.Name
        });

        return Paged(response, pageNumber, pageSize, result.TotalCount);
    }

    [HttpGet(ApiRoutes.Reports.GetById)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var report = await _unitOfWork.ItemReports.GetDetailsAsync(id);

        if (report == null)
            return Error("Report not found.", 404);

        var response = new ItemReportResponseDto
        {
            Id = report.Id,
            ItemName = report.ItemName,
            Color = report.Color,
            ConditionType = report.ConditionType,
            StatusType = report.StatusType,
            ReportType = report.ReportType,
            DateReported = report.DateReported,
            CategoryName = report.Category.Name,
            LocationName = report.Location.Name,
            UserName = report.User.Name
        };

        return Success(response);
    }

    [HttpPost(ApiRoutes.Reports.Create)]
    public async Task<IActionResult> Create([FromBody] ItemReportRequestDto dto)
    {
        var report = new ItemReport
        {
            ReportType = dto.ReportType,
            ItemName = dto.ItemName,
            Color = dto.Color,
            ConditionType = dto.ConditionType,
            DateReported = dto.DateReported,
            Description = dto.Description,
            LocationId = dto.LocationId,
            CategoryId = dto.CategoryId,
            UserId = GetUserId(),
            StatusType = enStatusType.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ItemReports.AddAsync(report);
        await _unitOfWork.SaveAsync();

        return Created(true, "Report created successfully.");
    }

    [HttpPut(ApiRoutes.Reports.Update)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ItemReportRequestDto dto)
    {
        var report = new ItemReport
        {
            Id = id,
            ReportType = dto.ReportType,
            ItemName = dto.ItemName,
            Color = dto.Color,
            ConditionType = dto.ConditionType,
            DateReported = dto.DateReported,
            Description = dto.Description,
            LocationId = dto.LocationId,
            CategoryId = dto.CategoryId
        };

        var updated = await _unitOfWork.ItemReports.UpdateReportAsync(report, GetUserId(), true);

        if (!updated)
            return Error("Failed to update report.", 400);

        await _unitOfWork.SaveAsync();

        return Success(true, "Report updated successfully.");
    }

    [HttpDelete(ApiRoutes.Reports.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var deleted = await _unitOfWork.ItemReports.DeleteReportAsync(id, GetUserId(), true);

        if (!deleted)
            return Error("Failed to delete report.", 400);

        await _unitOfWork.SaveAsync();

        return Success(true, "Report deleted successfully.");
    }

    [HttpPut(ApiRoutes.Reports.Cancel)]
    public async Task<IActionResult> Cancel([FromRoute] int id)
    {
        var canceled = await _unitOfWork.ItemReports.CancelReportAsync(id, GetUserId(), true);

        if (!canceled)
            return Error("Failed to cancel report.", 400);

        await _unitOfWork.SaveAsync();

        return Success(true, "Report canceled successfully.");
    }

    [HttpPut(ApiRoutes.Reports.ChangeStatus)]
    public async Task<IActionResult> ChangeStatus([FromRoute] int id, [FromQuery] enStatusType newStatus)
    {
        var changed = await _unitOfWork.ItemReports.ChangeStatusAsync(id, newStatus, GetUserId(), true);

        if (!changed)
            return Error("Failed to change report status.", 400);

        await _unitOfWork.SaveAsync();

        return Success(true, "Report status changed successfully.");
    }

    [HttpPut(ApiRoutes.Reports.ChangeReportType)]
    public async Task<IActionResult> ChangeReportType(
    [FromRoute] int id,
    [FromBody] ChangeReportTypeDto dto)
    {
        var changed = await _unitOfWork.ItemReports
            .ChangeReportTypeAsync(id, dto.ReportType, GetUserId(), true);

        if (!changed)
            return Error("Failed to change report type.");

        await _unitOfWork.SaveAsync();

        return Success(true, "Report type updated successfully.");
    }
}