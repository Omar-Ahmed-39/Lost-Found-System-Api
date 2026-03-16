using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.Locations;

public class LocationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public enLocationType LocationType { get; set; }
    public int DepartmentId { get; set; }
}
