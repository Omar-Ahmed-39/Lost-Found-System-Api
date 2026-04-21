using LostAndFound.Api.DTOs.ItemReports;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Filters;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers;

public class ReportsController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // =========================================
    // User / App Endpoints
    // =========================================

    [AllowAnonymous] // Allow anonymous access to view reports
    [HttpGet(ApiRoutes.Reports.GetAll)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? locationId,
        [FromQuery] enReportType? reportType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new ItemReportFilter
        {
            Search = search,
            CategoryId = categoryId,
            LocationId = locationId,
            ReportType = reportType
        };

        var result = await _unitOfWork.ItemReports.GetFilteredAsync(filter, pageNumber, pageSize, false);

        var response = result.Items.Select(r => new ReportListDto
        {
            Id = r.Id,
            ItemName = r.ItemName,
            ImagePath = r.Attachments.FirstOrDefault() != null
                ? r.Attachments.First().FilePath
                : string.Empty,
            DateReported = r.DateReported,
            ReportType = r.ReportType
        });

        return Paged(response, pageNumber, pageSize, result.TotalCount);
    }

    [Authorize]
    [HttpGet(ApiRoutes.Reports.GetById)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var report = await _unitOfWork.ItemReports.GetDetailsAsync(id);
        if (report == null)
            return Error("Report not found.", 404);

        var response = new UserReportDetailsDto
        {
            Id = report.Id,
            ItemName = report.ItemName,
            ImagePath = report.Attachments.FirstOrDefault()?.FilePath ?? string.Empty,
            ReportType = report.ReportType,
            LocationName = report.Location.Name,
            DateReported = report.DateReported
        };

        return Success(response);
    }

    [Authorize]
    [HttpGet(ApiRoutes.Reports.GetMyReports)]
    public async Task<IActionResult> GetMyReports()
    {
        var reports = await _unitOfWork.ItemReports.GetUserReportsAsync(GetUserId());

        var response = reports.Select(r => new MyReportDto
        {
            Id = r.Id,
            ItemName = r.ItemName,
            ImagePath = r.Attachments.FirstOrDefault()?.FilePath ?? string.Empty,
            DateReported = r.DateReported,
            ReportType = r.ReportType,
            StatusType = r.StatusType
        });

        return Success(response);
    }

    [Authorize]
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
            StatusType = enStatusType.Open
        };

        await _unitOfWork.ItemReports.AddAsync(report);
        await _unitOfWork.SaveAsync();

        return Created(new { report.Id }, "Report created successfully.");
    }

    [Authorize]
    [HttpPut(ApiRoutes.Reports.Update)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ItemReportRequestDto dto)
    {
        var report = new ItemReport
        {
            Id = id,
            ItemName = dto.ItemName,
            Color = dto.Color,
            ConditionType = dto.ConditionType,
            DateReported = dto.DateReported,
            Description = dto.Description,
            LocationId = dto.LocationId,
            CategoryId = dto.CategoryId
        };

        var updated = await _unitOfWork.ItemReports.UpdateReportAsync(report, GetUserId(), false);
        if (!updated)
            return Error("Failed to update report.", 400);

        await _unitOfWork.SaveAsync();
        return Success(true, "Report updated successfully.");
    }

    [Authorize]
    [HttpPut(ApiRoutes.Reports.Cancel)]
    public async Task<IActionResult> Cancel([FromRoute] int id)
    {
        var canceled = await _unitOfWork.ItemReports.CancelReportAsync(id, GetUserId(), false);
        if (!canceled)
            return Error("Failed to cancel report.", 400);

        await _unitOfWork.SaveAsync();
        return Success(true, "Report canceled successfully.");
    }

    // =========================================
    // Admin Endpoints
    // =========================================

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet(ApiRoutes.Reports.GetAllAdmin)]
    public async Task<IActionResult> GetAllAdmin(
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

        var result = await _unitOfWork.ItemReports.GetFilteredAsync(filter, pageNumber, pageSize, true);

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

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet(ApiRoutes.Reports.GetByIdAdmin)]
    public async Task<IActionResult> GetByIdAdmin([FromRoute] int id)
    {
        var report = await _unitOfWork.ItemReports.GetDetailsAsync(id);
        if (report == null)
            return Error("Report not found.", 404);

        var response = new AdminReportDetailsDto
        {
            Id = report.Id,
            ImagePath = report.Attachments.FirstOrDefault()?.FilePath ?? string.Empty,
            ReportType = report.ReportType,
            CategoryName = report.Category.Name,
            LocationName = report.Location.Name,
            ReportedBy = report.User.Name,
            DateReported = report.DateReported,
            Description = report.Description
        };

        return Success(response);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete(ApiRoutes.Reports.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var deleted = await _unitOfWork.ItemReports.DeleteReportAsync(id, GetUserId(), true);
        if (!deleted)
            return Error("Failed to delete report.", 400);

        await _unitOfWork.SaveAsync();
        return Success(true, "Report deleted successfully.");
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut(ApiRoutes.Reports.ChangeStatus)]
    public async Task<IActionResult> ChangeStatus([FromRoute] int id, [FromBody] ChangeStatusDto dto)
    {
        var changed = await _unitOfWork.ItemReports.ChangeStatusAsync(id, dto.StatusType, GetUserId(), true);
        if (!changed)
            return Error("Failed to change report status.", 400);

        await _unitOfWork.SaveAsync();
        return Success(true, "Report status changed successfully.");
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut(ApiRoutes.Reports.ChangeReportType)]
    public async Task<IActionResult> ChangeReportType([FromRoute] int id, [FromBody] ChangeReportTypeDto dto)
    {
        var changed = await _unitOfWork.ItemReports.ChangeReportTypeAsync(id, dto.ReportType, GetUserId(), true);
        if (!changed)
            return Error("Failed to change report type.", 400);

        await _unitOfWork.SaveAsync();
        return Success(true, "Report type updated successfully.");
    }
}