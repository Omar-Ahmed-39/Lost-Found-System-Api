namespace LostAndFound.Core.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation property for the many-to-many relationship with User
    public ICollection<User> Users { get; set; } = new List<User>();
}