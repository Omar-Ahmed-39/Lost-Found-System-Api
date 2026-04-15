namespace LostAndFound.Core.Entities;

public class Category : BaseEntity
{

    public string Name { get; set; } = string.Empty;

    //Navigation properties
    public ICollection<ItemReport> Reports { get; set; } = new List<ItemReport>();

}