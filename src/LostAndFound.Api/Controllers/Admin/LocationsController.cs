using LostAndFound.Api.DTOs.Locations;
using LostAndFound.Api.Filters;
using LostAndFound.Core.Constants;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Enums;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers.Admin;

[Authorize]
public class LocationsController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public LocationsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet(ApiRoutes.Locations.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var locations = await _unitOfWork.Locations.GetAllAsync(
            includes: l => l.Department
        );

        var response = locations.Select(l => new LocationResponseDto
        {
            Id = l.Id,
            Name = l.Name,
            LocationType = l.LocationType,
            DepartmentId = l.DepartmentId,
            DepartmentName = l.Department.Name
        });

        return Success(response);
    }

    [HttpGet(ApiRoutes.Locations.GetById)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var location = await _unitOfWork.Locations.GetAsync(
            l => l.Id == id,
            false,
            l => l.Department
        );

        if (location == null)
            return Error("Location not found.", 404);

        return Success(new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            LocationType = location.LocationType,
            DepartmentId = location.DepartmentId,
            DepartmentName = location.Department.Name
        });
    }

    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    [AuditLog("Created New Location")]
    [HttpPost(ApiRoutes.Locations.Create)]
    public async Task<IActionResult> Create([FromBody] LocationRequestDto dto)
    {
        var normalizedName = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
            return Error("Location name is required.", 400);

        if (!IsValidName(normalizedName))
            return Error("Location name must contain only letters and spaces.", 400);

        if (dto.DepartmentId <= 0)
            return Error("A valid department is required.", 400);

        if (!Enum.IsDefined(typeof(enLocationType), dto.LocationType))
            return Error("Invalid location type.", 400);

        var departmentExists = await _unitOfWork.Departments.ExistsAsync(d => d.Id == dto.DepartmentId);
        if (!departmentExists)
            return Error("Selected department does not exist.", 400);

        var exists = await _unitOfWork.Locations
            .ExistsAsync(l => l.Name.ToLower() == normalizedName.ToLower() && l.DepartmentId == dto.DepartmentId);

        if (exists)
            return Error("Location already exists in this department.", 400);

        var location = new Location
        {
            Name = normalizedName,
            LocationType = dto.LocationType,
            DepartmentId = dto.DepartmentId
        };

        await _unitOfWork.Locations.AddAsync(location);
        await _unitOfWork.SaveAsync();

        var department = await _unitOfWork.Departments.FindAsync(dto.DepartmentId);

        return Created(new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            LocationType = location.LocationType,
            DepartmentId = location.DepartmentId,
            DepartmentName = department?.Name ?? string.Empty
        }, "Location created successfully.");
    }

    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    [AuditLog("Updated Location")]
    [HttpPut(ApiRoutes.Locations.Update)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] LocationRequestDto dto)
    {
        var location = await _unitOfWork.Locations.FindAsync(id);
        if (location == null)
            return Error("Location not found.", 404);

        var normalizedName = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
            return Error("Location name is required.", 400);

        if (!IsValidName(normalizedName))
            return Error("Location name must contain only letters and spaces.", 400);

        if (dto.DepartmentId <= 0)
            return Error("A valid department is required.", 400);

        if (!Enum.IsDefined(typeof(enLocationType), dto.LocationType))
            return Error("Invalid location type.", 400);

        var departmentExists = await _unitOfWork.Departments.ExistsAsync(d => d.Id == dto.DepartmentId);
        if (!departmentExists)
            return Error("Selected department does not exist.", 400);

        var exists = await _unitOfWork.Locations
            .ExistsAsync(l => l.Name.ToLower() == normalizedName.ToLower()
                           && l.DepartmentId == dto.DepartmentId
                           && l.Id != id);

        if (exists)
            return Error("Location name already exists in this department.", 400);

        location.Name = normalizedName;
        location.LocationType = dto.LocationType;
        location.DepartmentId = dto.DepartmentId;

        await _unitOfWork.SaveAsync();

        var department = await _unitOfWork.Departments.FindAsync(dto.DepartmentId);

        return Success(new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            LocationType = location.LocationType,
            DepartmentId = location.DepartmentId,
            DepartmentName = department?.Name ?? string.Empty
        }, "Location updated successfully.");
    }

    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    [AuditLog("Deleted Location")]
    [HttpDelete(ApiRoutes.Locations.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var location = await _unitOfWork.Locations.FindAsync(id);
        if (location == null)
            return Error("Location not found.", 404);

        var hasReports = await _unitOfWork.ItemReports.ExistsAsync(r => r.LocationId == id);
        if (hasReports)
            return Error("Cannot delete location because it has related reports.", 400);

        var hasHandovers = await _unitOfWork.Handovers.ExistsAsync(h => h.LocationId == id);
        if (hasHandovers)
            return Error("Cannot delete location because it has related handovers.", 400);

        _unitOfWork.Locations.Remove(location);
        await _unitOfWork.SaveAsync();

        return Success(true, "Location deleted successfully.");
    }
}