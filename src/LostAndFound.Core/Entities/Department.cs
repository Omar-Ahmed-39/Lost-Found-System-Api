namespace LostAndFound.Core.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Foreign key to University
    public int UniversityId { get; set; }

    // Navigation property
    public University University { get; set; } = default!;
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}