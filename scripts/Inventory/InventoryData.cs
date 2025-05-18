namespace EscapeFromZone.scripts.Inventory;


public class InventoryData
{
    public List<SlotData> Slots { get; set; } = new();
    public List<EquipmentSlotData> EquipmentSlots { get; set; } = new();

    public bool HaveWeaponInSlot(int slotNumber)
    {
        if (slotNumber < 0 || slotNumber >= EquipmentSlots.Count)
        {
            GD.Print("Нет такого слота");
            return false;
        }

        if (EquipmentSlots[slotNumber].SlotCategory != EquipmentSlot.CategoryType.Weapon)
        {
            GD.Print("У слота другая категория");
            return false;
        }

        if (EquipmentSlots[slotNumber].ItemData != null && EquipmentSlots[slotNumber].ItemData.Category == "Weapon")
        {
            return true;
        }

        return false;
    }

    public int GetWeaponIDFromSlot(int slotNumber) => EquipmentSlots[slotNumber].ItemData.ItemID;
}