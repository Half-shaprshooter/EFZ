namespace EscapeFromZone.scripts.Items;

public class ItemData
{
    public int ItemID { get; set; }
    public List<Vector2I> ItemGrids { get; set; }
    public string Category { get; set; }

    public ItemData(Item item)
    {
        ItemID = item.ItemID;
        ItemGrids = item.ItemGrids;
        Category = item.Category;
    }
}