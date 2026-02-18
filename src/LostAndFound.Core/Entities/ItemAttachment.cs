namespace LostAndFound.Core.Entities;

public class ItemAttachment
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Foreign key
    public int ReportId { get; set; }

    // Navigation properties
    public ItemReport ItemReport { get; set; } = default!;
}