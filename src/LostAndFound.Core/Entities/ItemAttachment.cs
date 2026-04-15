namespace LostAndFound.Core.Entities;

public class ItemAttachment : BaseEntity
{

    public string FilePath { get; set; } = string.Empty;


    // Foreign key
    public int ReportId { get; set; }

    // Navigation properties
    public ItemReport ItemReport { get; set; } = default!;
}