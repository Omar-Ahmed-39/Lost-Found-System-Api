using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Core.Entities;

public class Location
{
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string LocationName { get; set; } = string.Empty;  
    public LocationType LocationType { get; set; }

    // Foreign key 
    public int DepartmentId { get; set; }

    // Navigation property
    public Department Department { get; set; } = default!;
}
