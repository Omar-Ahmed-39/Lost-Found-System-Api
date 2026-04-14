namespace LostAndFound.Core.Entities;

public class AuditLog : BaseEntity
{

    public string Action {get; set;} = string.Empty; // ex :- Aprroved Claim CLM-001
    public string Target {get; set;} = string.Empty; // ex :- Ali Salem - Iphone 17 plus 
    public string IpAddress {get; set;} = string.Empty;


    public int? UserId {get; set;}
    public User? User {get; set;}
}