using LostAndFound.Api.DTOs.Categories;
using LostAndFound.Api.Filters;
using LostAndFound.Core.Constants;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers.Admin;

[Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
public class CategoriesController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet(ApiRoutes.Categories.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();

        var response = categories.Select(c => new CategoryResponseDto
        {
            Id = c.Id,
            Name = c.Name
        });

        return Success(response);
    }

    [HttpGet(ApiRoutes.Categories.GetById)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var category = await _unitOfWork.Categories.FindAsync(id);
        if (category == null)
            return Error("Category not found.", 404);

        return Success(new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        });
    }

    [AuditLog("Created New Category")]
    [HttpPost(ApiRoutes.Categories.Create)]
    public async Task<IActionResult> Create([FromBody] CategoryRequestDto dto)
    {
        var normalizedName = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
            return Error("Category name is required.", 400);

        var exists = await _unitOfWork.Categories.ExistsAsync(c =>
            c.Name.ToLower() == normalizedName.ToLower());

        if (exists)
            return Error("Category already exists.", 400);

        var category = new Category
        {
            Name = normalizedName
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveAsync();

        return Created(new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        }, "Category created successfully.");
    }

    [AuditLog("Updated Category")]
    [HttpPut(ApiRoutes.Categories.Update)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CategoryRequestDto dto)
    {
        var category = await _unitOfWork.Categories.FindAsync(id);
        if (category == null)
            return Error("Category not found.", 404);

        var normalizedName = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
            return Error("Category name is required.", 400);

        var exists = await _unitOfWork.Categories.ExistsAsync(c =>
            c.Name.ToLower() == normalizedName.ToLower() &&
            c.Id != id);

        if (exists)
            return Error("Category name already exists.", 400);

        category.Name = normalizedName;
        await _unitOfWork.SaveAsync();

        return Success(new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        }, "Category updated successfully.");
    }

    [AuditLog("Deleted Category")]
    [HttpDelete(ApiRoutes.Categories.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var category = await _unitOfWork.Categories.FindAsync(id);
        if (category == null)
            return Error("Category not found.", 404);

        var hasReports = await _unitOfWork.ItemReports.ExistsAsync(r => r.CategoryId == id);
        if (hasReports)
            return Error("Cannot delete category because it has related reports.", 400);

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveAsync();

        return Success(true, "Category deleted successfully.");
    }
}