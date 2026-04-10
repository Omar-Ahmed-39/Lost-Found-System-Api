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

    /// <summary>Returns all categories.</summary>
    [HttpGet(ApiRoutes.Categories.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return Success(categories.Select(ToDto));
    }

    /// <summary>Returns a single category by ID.</summary>
    [HttpGet(ApiRoutes.Categories.GetById)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var category = await _unitOfWork.Categories.FindAsync(id);
        if (category is null)
            return Error("Category not found.", StatusCodes.Status404NotFound);

        return Success(ToDto(category));
    }

    /// <summary>Creates a new category.</summary>
    [HttpPost(ApiRoutes.Categories.Create)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CategoryRequestDto request)
    {
        var category = new Category { Name = request.Name };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveAsync();

        return Created(ToDto(category));
    }

    /// <summary>Updates an existing category.</summary>
    [HttpPut(ApiRoutes.Categories.Update)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CategoryRequestDto request)
    {
        var category = await _unitOfWork.Categories.FindAsync(id);
        if (category is null)
            return Error("Category not found.", StatusCodes.Status404NotFound);

        category.Name = request.Name;

        await _unitOfWork.SaveAsync();
        return Success(ToDto(category));
    }

    /// <summary>Deletes a category by ID.</summary>
    [HttpDelete(ApiRoutes.Categories.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var category = await _unitOfWork.Categories.FindAsync(id);
        if (category is null)
            return Error("Category not found.", StatusCodes.Status404NotFound);

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveAsync();

        return Success(true, "Category deleted successfully.");
    }

    private static CategoryResponseDto ToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name
    };
}
