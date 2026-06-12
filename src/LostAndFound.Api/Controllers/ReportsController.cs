using LostAndFound.Api.DTOs.ItemReports;
using LostAndFound.Core.Constants;
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
    private readonly IFileService _fileService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IUnitOfWork unitOfWork,
        IFileService fileService,
        IServiceScopeFactory scopeFactory,
        ILogger<ReportsController> logger)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _scopeFactory = scopeFactory;
        _logger = logger;
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

            ImagePath = r.Attachments
        .Select(a => a.FilePath)
        .FirstOrDefault() ?? string.Empty,

            ReportType = r.ReportType.ToString(),

            Status = r.StatusType.ToString(),

            LocationName = r.Location.Name,

            ReporterName = r.User.Name,

            DateReported = r.DateReported,
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
            Images = report.Attachments.Select(a => new ImageDto
            {
                Id = a.Id,
                Path = a.FilePath

            }).ToList(),

            ReportType = report.ReportType.ToString(),
            LocationName = report.Location.Name,
            DateReported = report.DateReported,

            Description = report.Description
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
    public async Task<IActionResult> Create([FromForm] ItemReportRequestDto dto)
    {
        var itemName = dto.ItemName?.Trim();

        if (string.IsNullOrWhiteSpace(itemName))
            return Error("Item name is required.", 400);

        if (dto.LocationId <= 0)
            return Error("A valid location is required.", 400);

        if (dto.CategoryId <= 0)
            return Error("A valid category is required.", 400);

        if (dto.Images != null && dto.Images.Count > 5)
            return Error("Max 5 images allowed", 400);

        if (!Enum.IsDefined(typeof(enReportType), dto.ReportType))
            return Error("Invalid report type.", 400);

        if (!Enum.IsDefined(typeof(enConditionType), dto.ConditionType))
            return Error("Invalid condition type.", 400);

        var locationExists = await _unitOfWork.Locations.ExistsAsync(l => l.Id == dto.LocationId);
        if (!locationExists)
            return Error("Selected location does not exist.", 400);

        var categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            return Error("Selected category does not exist.", 400);

        var report = new ItemReport
        {
            ReportType = dto.ReportType,
            ItemName = itemName,
            Color = dto.Color?.Trim(),
            ConditionType = dto.ConditionType,
            DateReported = dto.DateReported,
            Description = dto.Description?.Trim(),
            LocationId = dto.LocationId,
            CategoryId = dto.CategoryId,
            UserId = GetUserId(),
            StatusType = enStatusType.Open
        };

        await _unitOfWork.ItemReports.AddAsync(report);
        await _unitOfWork.SaveAsync();

        // ✅ Upload images
        if (dto.Images != null && dto.Images.Any())
        {
            foreach (var image in dto.Images)
            {
                var extension = Path.GetExtension(image.FileName).ToLower();

                if (!FileSettings.AllowedExtensions.Contains(extension))
                    return Error("Invalid image format.", 400);

                if (image.Length > FileSettings.MaxSizeMB * 1024 * 1024)
                    return Error("Image size too large.", 400);

                if (image.Length > 0)
                {
                    var filePath = await _fileService.UploadFileAsync(
                        image.OpenReadStream(),
                        image.FileName,
                        "Reports"
                    );

                    var attachment = new ItemAttachment
                    {
                        FilePath = filePath,
                        ReportId = report.Id
                    };

                    await _unitOfWork.ItemAttachments.AddAsync(attachment);
                }
            }

            await _unitOfWork.SaveAsync();
        }

        // ✅ Background matching (from main branch)
        var reportId = report.Id;

        _ = Task.Run(async () =>
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();

            try
            {
                await matchingService.ProcessMatchesForReportAsync(reportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background matching failed for report {ReportId}", reportId);
            }
        });

        return Created(new { report.Id }, "Report created successfully.");
    }

    [Authorize]
    [HttpPut(ApiRoutes.Reports.Update)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromForm] UpdateReportDto dto)
    {
        var report = await _unitOfWork.ItemReports.GetDetailsAsync(id);
        if (report == null)
            return Error("Report not found.", 404);

        var locationExists = await _unitOfWork.Locations.ExistsAsync(l => l.Id == dto.LocationId);
        if (!locationExists)
            return Error("Selected location does not exist.", 400);

        var categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            return Error("Selected category does not exist.", 400);

        var currentUserId = GetUserId();
        bool isAdminOrSuperAdmin = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.SuperAdmin);
        bool isReportOwner = report.UserId == currentUserId;

        if (!isAdminOrSuperAdmin && !isReportOwner)
        {
            return StatusCode(403, ApiResponse<object>.Failure("You do not have permission to edit this report."));
        }

        if (!Enum.IsDefined(typeof(enReportType), dto.ReportType))
            return Error("Invalid report type.", 400);

        if (!Enum.IsDefined(typeof(enConditionType), dto.ConditionType))
            return Error("Invalid condition type.", 400);


        var itemName = dto.ItemName?.Trim();
        if (string.IsNullOrWhiteSpace(itemName))
            return Error("Item name is required.", 400);

        report.ItemName = itemName;
        report.Color = dto.Color?.Trim();
        report.ConditionType = dto.ConditionType;
        report.Description = dto.Description?.Trim();
        report.LocationId = dto.LocationId;
        report.CategoryId = dto.CategoryId;
        report.DateReported = dto.DateReported;
        report.UpdatedAt = DateTime.UtcNow;

        if (dto.DeletedImageIds != null && dto.DeletedImageIds.Any())
        {
            var attachmentsToDelete = report.Attachments
                .Where(a => dto.DeletedImageIds.Contains(a.Id) && a.ReportId == report.Id)
                .ToList();

            foreach (var attachment in attachmentsToDelete)
            {
                await _fileService.DeleteFileAsync(attachment.FilePath);
                _unitOfWork.ItemAttachments.Remove(attachment);
            }
        }

        if (dto.NewImages != null && dto.NewImages.Any())
        {
            foreach (var image in dto.NewImages)
            {
                var extension = Path.GetExtension(image.FileName).ToLower();

                if (!FileSettings.AllowedExtensions.Contains(extension))
                    return Error("Invalid image format.", 400);

                if (image.Length > FileSettings.MaxSizeMB * 1024 * 1024)
                    return Error("Image size too large.", 400);

                var filePath = await _fileService.UploadFileAsync(
                    image.OpenReadStream(),
                    image.FileName,
                    "Reports"
                );

                var attachment = new ItemAttachment
                {
                    FilePath = filePath,
                    ReportId = report.Id
                };

                await _unitOfWork.ItemAttachments.AddAsync(attachment);
            }
        }

        await _unitOfWork.SaveAsync();

        return Success(true, "Report updated with images successfully.");
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

    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
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

    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
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

    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    [HttpDelete(ApiRoutes.Reports.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var report = await _unitOfWork.ItemReports.GetDetailsAsync(id);

        if (report == null)
            return Error("Report not found.", 404);

        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Delete physical files
            foreach (var attachment in report.Attachments)
            {
                await _fileService.DeleteFileAsync(attachment.FilePath);
            }

            // 2. Delete attachments from DB
            if (report.Attachments.Any())
            {
                _unitOfWork.ItemAttachments.RemoveRange(report.Attachments);
            }

            // 3. Delete report
            _unitOfWork.ItemReports.Remove(report);

            // 4. Save changes
            await _unitOfWork.SaveAsync();

            // 5. Commit
            await transaction.CommitAsync();

            return Success(true, "Report and images deleted successfully.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Error("Failed to delete report.", 500);
        }
    }

    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    [HttpPut(ApiRoutes.Reports.ChangeStatus)]
    public async Task<IActionResult> ChangeStatus([FromRoute] int id, [FromBody] ChangeStatusDto dto)
    {
        if (!Enum.IsDefined(typeof(enStatusType), dto.StatusType))
            return Error("Invalid status type.", 400);

        var changed = await _unitOfWork.ItemReports.ChangeStatusAsync(id, dto.StatusType, GetUserId(), true);
        if (!changed)
            return Error("Failed to change report status.", 400);

        await _unitOfWork.SaveAsync();
        return Success(true, "Report status changed successfully.");
    }

    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    [HttpPut(ApiRoutes.Reports.ChangeReportType)]
    public async Task<IActionResult> ChangeReportType([FromRoute] int id, [FromBody] ChangeReportTypeDto dto)
    {
        if (!Enum.IsDefined(typeof(enReportType), dto.ReportType))
            return Error("Invalid report type.", 400);

        var changed = await _unitOfWork.ItemReports.ChangeReportTypeAsync(id, dto.ReportType, GetUserId(), true);
        if (!changed)
            return Error("Failed to change report type.", 400);

        await _unitOfWork.SaveAsync();
        return Success(true, "Report type updated successfully.");
    }
}