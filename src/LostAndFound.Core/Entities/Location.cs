using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Core.Entities;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public enLocationType LocationType { get; set; }

    // Foreign key 
    public int DepartmentId { get; set; }

    // Navigation property
    public Department Department { get; set; } = default!;
    public ICollection<Handover> Handovers { get; set; } = new List<Handover>();
    public ICollection<ItemReport> Reports { get; set; } = new List<ItemReport>();
}