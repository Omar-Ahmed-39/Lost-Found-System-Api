namespace LostAndFound.Core.Entities;

public class ItemReport : BaseEntity
{

    public enReportType ReportType { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public enConditionType ConditionType { get; set; }
    public enStatusType StatusType { get; set; }
    public DateTime DateReported { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AdminNotes { get; set; } = string.Empty;


    //Foreign key
    public int UserId { get; set; }
    public int LocationId { get; set; }
    public int CategoryId { get; set; }

    //Navigation properties
    public User User { get; set; } = default!;
    public Location Location { get; set; } = default!;
    public Category Category { get; set; } = default!;
    public ICollection<ItemAttachment> Attachments { get; set; } = new List<ItemAttachment>();
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    public ICollection<Match> LostMatches { get; set; } = new List<Match>();
    public ICollection<Match> FoundMatches { get; set; } = new List<Match>();
}
