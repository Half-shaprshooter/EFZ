using Godot;
using System;

public partial class InventoryManager : Node
{
    public static Inventory PlayerInventory { get; set; }

    public static void UseInventoryItem(int slotId)
    {
        PlayerInventory.UseItemFromSlot(slotId);
    }
}
