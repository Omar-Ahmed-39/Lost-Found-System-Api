namespace LostAndFound.Core.Entities;

public class University : BaseEntity
{

    public string Name { get; set; } = string.Empty;

    // Navigation property 
    public ICollection<Department> Departments { get; set; } = new List<Department>();

}