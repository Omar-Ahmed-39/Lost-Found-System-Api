using Microsoft.AspNetCore.Identity;

namespace LostAndFound.Core.Entities;

public class Role : IdentityRole<int>
{
    // Navigation property for the many-to-many relationship with User
    public ICollection<User> Users { get; set; } = new List<User>();
}