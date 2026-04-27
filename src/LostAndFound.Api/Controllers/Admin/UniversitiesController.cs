using LostAndFound.Api.DTOs.Universities;
using LostAndFound.Api.Filters;
using LostAndFound.Core.Constants;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers.Admin;

[Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
public class UniversitiesController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public UniversitiesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet(ApiRoutes.Universities.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var universities = await _unitOfWork.Universities.GetAllAsync();

        var response = universities.Select(u => new UniversityResponseDto
        {
            Id = u.Id,
            Name = u.Name
        });

        return Success(response);
    }

    [HttpGet(ApiRoutes.Universities.GetById)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var university = await _unitOfWork.Universities.FindAsync(id);
        if (university == null)
            return Error("University not found.", 404);

        return Success(new UniversityResponseDto
        {
            Id = university.Id,
            Name = university.Name
        });
    }

    [AuditLog("Created New University")]
    [HttpPost(ApiRoutes.Universities.Create)]
    public async Task<IActionResult> Create([FromBody] UniversityRequestDto dto)
    {
        var normalizedName = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
            return Error("University name is required.", 400);

        if (!IsValidName(normalizedName))
            return Error("University name must contain only letters and spaces.", 400);

        var exists = await _unitOfWork.Universities
            .ExistsAsync(u => u.Name.ToLower() == normalizedName.ToLower());

        if (exists)
            return Error("University already exists.", 400);

        var university = new University
        {
            Name = normalizedName
        };

        await _unitOfWork.Universities.AddAsync(university);
        await _unitOfWork.SaveAsync();

        return Created(new UniversityResponseDto
        {
            Id = university.Id,
            Name = university.Name
        }, "University created successfully.");
    }

    [AuditLog("Updated University")]
    [HttpPut(ApiRoutes.Universities.Update)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UniversityRequestDto dto)
    {
        var university = await _unitOfWork.Universities.FindAsync(id);
        if (university == null)
            return Error("University not found.", 404);

        var normalizedName = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
            return Error("University name is required.", 400);

        if (!IsValidName(normalizedName))
            return Error("University name must contain only letters and spaces.", 400);

        var exists = await _unitOfWork.Universities
            .ExistsAsync(u => u.Name.ToLower() == normalizedName.ToLower() && u.Id != id);

        if (exists)
            return Error("University name already exists.", 400);

        university.Name = normalizedName;
        await _unitOfWork.SaveAsync();

        return Success(new UniversityResponseDto
        {
            Id = university.Id,
            Name = university.Name
        }, "University updated successfully.");
    }

    [AuditLog("Deleted University")]
    [HttpDelete(ApiRoutes.Universities.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var university = await _unitOfWork.Universities.FindAsync(id);
        if (university == null)
            return Error("University not found.", 404);

        var hasDepartments = await _unitOfWork.Departments.ExistsAsync(d => d.UniversityId == id);
        if (hasDepartments)
            return Error("Cannot delete university because it has related departments.", 400);

        _unitOfWork.Universities.Remove(university);
        await _unitOfWork.SaveAsync();

        return Success(true, "University deleted successfully.");
    }
}