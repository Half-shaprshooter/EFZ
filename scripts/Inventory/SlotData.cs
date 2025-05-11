using EscapeFromZone.scripts.Items;

namespace EscapeFromZone.scripts.Inventory;

public class SlotData
{
    public int SlotId { get; set; }
    public Slot.States State { get; set; }
    public ItemData ItemData { get; set; }
}