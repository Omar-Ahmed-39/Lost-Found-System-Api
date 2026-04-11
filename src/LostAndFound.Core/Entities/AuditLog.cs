namespace LostAndFound.Core.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string Action {get; set;} = string.Empty; // ex :- Aprroved Claim CLM-001
    public string Target {get; set;} = string.Empty; // ex :- Ali Salem - Iphone 17 plus 
    public string IpAddress {get; set;} = string.Empty;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

    public int? UserId {get; set;}
    public User? User {get; set;}
}