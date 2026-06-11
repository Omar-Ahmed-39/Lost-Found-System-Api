namespace LostAndFound.Api.DTOs.Admin;

public class AuditLogDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? AdminName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}
