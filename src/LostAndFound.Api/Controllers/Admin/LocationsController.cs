using System.ComponentModel.DataAnnotations;
using LostAndFound.Api.DTOs.Locations;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.Api.Controllers.Admin;

[Authorize(Roles = "Admin,SuperAdmin")]
public class LocationsController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public LocationsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>Returns all locations.</summary>
    [HttpGet(ApiRoutes.Locations.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var locations = await _unitOfWork.Locations.GetAllAsync();
        return Success(locations.Select(ToDto));
    }

    /// <summary>Returns a single location by ID.</summary>
    [HttpGet(ApiRoutes.Locations.GetById)]
    [ProducesResponseType(typeof(ApiResponse<LocationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var location = await _unitOfWork.Locations.FindAsync(id);
        if (location is null)
            return Error("Location not found.", StatusCodes.Status404NotFound);

        return Success(ToDto(location));
    }

    /// <summary>Creates a new location.</summary>
    [HttpPost(ApiRoutes.Locations.Create)]
    [ProducesResponseType(typeof(ApiResponse<LocationResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] LocationRequestDto request)
    {
        var location = new Location
        {
            Name = request.Name,
            LocationType = request.LocationType,
            DepartmentId = request.DepartmentId
        };

        await _unitOfWork.Locations.AddAsync(location);
        await _unitOfWork.SaveAsync();

        return Created(ToDto(location));
    }

    /// <summary>Updates an existing location.</summary>
    [HttpPut(ApiRoutes.Locations.Update)]
    [ProducesResponseType(typeof(ApiResponse<LocationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] LocationRequestDto request)
    {
        var location = await _unitOfWork.Locations.FindAsync(id);
        if (location is null)
            return Error("Location not found.", StatusCodes.Status404NotFound);

        location.Name = request.Name;
        location.LocationType = request.LocationType;
        location.DepartmentId = request.DepartmentId;

        await _unitOfWork.SaveAsync();
        return Success(ToDto(location));
    }

    /// <summary>Deletes a location by ID.</summary>
    [HttpDelete(ApiRoutes.Locations.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var location = await _unitOfWork.Locations.FindAsync(id);
        if (location is null)
            return Error("Location not found.", StatusCodes.Status404NotFound);

        _unitOfWork.Locations.Remove(location);
        await _unitOfWork.SaveAsync();

        return Success(true, "Location deleted successfully.");
    }

    private static LocationResponseDto ToDto(Location location) => new()
    {
        Id = location.Id,
        Name = location.Name,
        LocationType = location.LocationType,
        DepartmentId = location.DepartmentId
    };
}
