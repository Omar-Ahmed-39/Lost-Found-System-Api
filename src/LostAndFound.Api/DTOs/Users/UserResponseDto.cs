namespace LostAndFound.Api.DTOs.Users;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}
