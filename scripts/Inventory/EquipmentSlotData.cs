using EscapeFromZone.scripts.Items;

namespace EscapeFromZone.scripts.Inventory;

public class EquipmentSlotData
{
    public int SlotId { get; set; }
    public EquipmentSlot.States State { get; set; }
    public ItemData ItemData { get; set; }
    public EquipmentSlot.CategoryType SlotCategory { get; set; }
}