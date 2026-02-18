namespace LostAndFound.Core.Entities;

public class Role
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;

    // Navigation property for the many-to-many relationship with User 
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
