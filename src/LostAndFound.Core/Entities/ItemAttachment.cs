namespace LostAndFound.Core.Entities;

public class ItemAttachment
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}