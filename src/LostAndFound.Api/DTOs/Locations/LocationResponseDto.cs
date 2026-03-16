using LostAndFound.Core.Enums;

namespace LostAndFound.Api.DTOs.Locations;

public class LocationResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public enLocationType LocationType { get; set; }
    public int DepartmentId { get; set; }
}
