using LostAndFound.Core.Enums;

namespace LostAndFound.Core.Entities;

public class ItemReport
{
    public int Id { get; set; }
    public enReportType ReportType { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public enConditionType ConditionType { get; set; }
    public enStatusType StatusType { get; set; }
    public DateTime DateReported { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AdminNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    //Foreign key
    public int UserId { get; set; }
    public int LocationId { get; set; }
    public int CategoryId { get; set; }
    public int ImageId { get; set; }

    //Navigation properties
    public User? User { get; set; }
    public Location? Location { get; set; }
    public Category? Category { get; set; }
    public ICollection<ItemAttachment> Attachments { get; set; } = new List<ItemAttachment>();
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    public ICollection<Match> LostId { get; set; } = new List<Match>();
    public ICollection<Match> FoundId { get; set; } = new List<Match>();
}
