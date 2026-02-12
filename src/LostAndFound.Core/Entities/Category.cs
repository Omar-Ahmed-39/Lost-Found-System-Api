namespace LostAndFound.Core.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    //Navigation properties
    public ICollection<ItemReport> Reports { get; set; } = new List<ItemReport>();

}