using LostAndFound.Api.DTOs.Departments;
using LostAndFound.Api.Filters;
using LostAndFound.Core.Constants;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers.Admin;

[Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
public class DepartmentsController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [AllowAnonymous]
    [HttpGet(ApiRoutes.Departments.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _unitOfWork.Departments.GetAllAsync(
            includes: d => d.University
        );

        var response = departments.Select(d => new DepartmentResponseDto
        {
            Id = d.Id,
            Name = d.Name,
            UniversityId = d.UniversityId,
            UniversityName = d.University.Name
        });

        return Success(response);
    }

    [HttpGet(ApiRoutes.Departments.GetById)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var department = await _unitOfWork.Departments.GetAsync(
            d => d.Id == id,
            false,
            d => d.University
        );

        if (department == null)
            return Error("Department not found.", 404);

        return Success(new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            UniversityId = department.UniversityId,
            UniversityName = department.University.Name
        });
    }

    [AuditLog("Created New Department")]
    [HttpPost(ApiRoutes.Departments.Create)]
    public async Task<IActionResult> Create([FromBody] DepartmentRequestDto dto)
    {
        var normalizedName = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
            return Error("Department name is required.", 400);

        if (!IsValidName(normalizedName))
            return Error("Department name must contain only letters and spaces.", 400);

        if (dto.UniversityId <= 0)
            return Error("A valid university is required.", 400);

        var universityExists = await _unitOfWork.Universities.ExistsAsync(u => u.Id == dto.UniversityId);
        if (!universityExists)
            return Error("Selected university does not exist.", 400);

        var exists = await _unitOfWork.Departments
            .ExistsAsync(d => d.Name.ToLower() == normalizedName.ToLower() && d.UniversityId == dto.UniversityId);

        if (exists)
            return Error("Department already exists in this university.", 400);

        var department = new Department
        {
            Name = normalizedName,
            UniversityId = dto.UniversityId
        };

        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveAsync();

        var university = await _unitOfWork.Universities.FindAsync(dto.UniversityId);

        return Created(new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            UniversityId = department.UniversityId,
            UniversityName = university?.Name ?? string.Empty
        }, "Department created successfully.");
    }

    [AuditLog("Updated Department")]
    [HttpPut(ApiRoutes.Departments.Update)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] DepartmentRequestDto dto)
    {
        var department = await _unitOfWork.Departments.FindAsync(id);
        if (department == null)
            return Error("Department not found.", 404);

        var normalizedName = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
            return Error("Department name is required.", 400);

        if (!IsValidName(normalizedName))
            return Error("Department name must contain only letters and spaces.", 400);

        if (dto.UniversityId <= 0)
            return Error("A valid university is required.", 400);

        var universityExists = await _unitOfWork.Universities.ExistsAsync(u => u.Id == dto.UniversityId);
        if (!universityExists)
            return Error("Selected university does not exist.", 400);

        var exists = await _unitOfWork.Departments
            .ExistsAsync(d => d.Name.ToLower() == normalizedName.ToLower()
                           && d.UniversityId == dto.UniversityId
                           && d.Id != id);

        if (exists)
            return Error("Department name already exists in this university.", 400);

        department.Name = normalizedName;
        department.UniversityId = dto.UniversityId;

        await _unitOfWork.SaveAsync();

        var university = await _unitOfWork.Universities.FindAsync(dto.UniversityId);

        return Success(new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            UniversityId = department.UniversityId,
            UniversityName = university?.Name ?? string.Empty
        }, "Department updated successfully.");
    }

    [AuditLog("Deleted Department")]
    [HttpDelete(ApiRoutes.Departments.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var department = await _unitOfWork.Departments.FindAsync(id);
        if (department == null)
            return Error("Department not found.", 404);

        var hasLocations = await _unitOfWork.Locations.ExistsAsync(l => l.DepartmentId == id);
        if (hasLocations)
            return Error("Cannot delete department because it has related locations.", 400);

        _unitOfWork.Departments.Remove(department);
        await _unitOfWork.SaveAsync();

        return Success(true, "Department deleted successfully.");
    }
}