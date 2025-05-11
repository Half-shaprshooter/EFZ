namespace EscapeFromZone.scripts.Inventory;


public class InventoryData
{
    public List<SlotData> Slots { get; set; } = new();
    public List<EquipmentSlotData> EquipmentSlots { get; set; } = new();
}