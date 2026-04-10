using LostAndFound.Api.DTOs.Categories;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers.Admin;

[Authorize(Roles = "Admin,SuperAdmin")]
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
        
        var response = categories.Select(x => new CategoryResponseDto
        {
            Id = x.Id,
            Name = x.Name
        });

        return Success(response);
    }

    [HttpGet(ApiRoutes.Categories.GetById)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _unitOfWork.Categories.FindAsync(id);
        if (category == null)
            return Error("Category not found", 404);

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };

        return Success(response);
    }

    [HttpPost(ApiRoutes.Categories.Create)]
    public async Task<IActionResult> Create([FromBody] CategoryRequestDto request)
    {
        var category = new Category
        {
            Name = request.Name
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveAsync();

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };

        return Created(response);
    }

    [HttpPut(ApiRoutes.Categories.Update)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryRequestDto request)
    {
        var category = await _unitOfWork.Categories.FindAsync(id);
        if (category == null)
            return Error("Category not found", 404);

        category.Name = request.Name;

        await _unitOfWork.SaveAsync();

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };

        return Success(response);
    }

    [HttpDelete(ApiRoutes.Categories.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _unitOfWork.Categories.FindAsync(id);
        if (category == null)
            return Error("Category not found", 404);

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveAsync();

        return Success(true, "Category deleted successfully.");
    }
}
