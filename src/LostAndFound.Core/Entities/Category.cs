namespace LostAndFound.Core.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    //Foreign key
    public int ReportId { get; set; }

    //Navigation properties
    public ItemReport Report { get; set; } = default!;

}