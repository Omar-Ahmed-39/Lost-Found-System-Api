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

    [HttpGet(ApiRoutes.Locations.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var locations = await _unitOfWork.Locations.GetAllAsync();
        
        var response = locations.Select(x => new LocationResponseDto
        {
            Id = x.Id,
            Name = x.Name,
            LocationType = x.LocationType,
            DepartmentId = x.DepartmentId
        });

        return Success(response);
    }

    [HttpGet(ApiRoutes.Locations.GetById)]
    public async Task<IActionResult> GetById(int id)
    {
        var location = await _unitOfWork.Locations.FindAsync(id);
        if (location == null)
            return Error("Location not found", 404);

        var response = new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            LocationType = location.LocationType,
            DepartmentId = location.DepartmentId
        };

        return Success(response);
    }

    [HttpPost(ApiRoutes.Locations.Create)]
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

        var response = new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            LocationType = location.LocationType,
            DepartmentId = location.DepartmentId
        };

        return Created(response);
    }

    [HttpPut(ApiRoutes.Locations.Update)]
    public async Task<IActionResult> Update(int id, [FromBody] LocationRequestDto request)
    {
        var location = await _unitOfWork.Locations.FindAsync(id);
        if (location == null)
            return Error("Location not found", 404);

        location.Name = request.Name;
        location.LocationType = request.LocationType;
        location.DepartmentId = request.DepartmentId;

        _unitOfWork.Locations.Update(location);
        await _unitOfWork.SaveAsync();

        var response = new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            LocationType = location.LocationType,
            DepartmentId = location.DepartmentId
        };

        return Success(response);
    }

    [HttpDelete(ApiRoutes.Locations.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _unitOfWork.Locations.FindAsync(id);
        if (location == null)
            return Error("Location not found", 404);

        _unitOfWork.Locations.Remove(location);
        await _unitOfWork.SaveAsync();

        return Success(true, "Location deleted successfully.");
    }
}
