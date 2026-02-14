using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Core.Entities;

public class University
{
    public int Id { get; set; }
    public string UniversityName { get; set; } = string.Empty;

    // Navigation property 
    public ICollection<Department> Departments { get; set; } = new List<Department>();

}
