using Godot;
using System;
using EscapeFromZone.scripts.Items;

public partial class ItemUseHandler : Control
{

    public void UseItem(Item item)
    {
        switch (item.ItemID)
        {
            case 9: // Бинт
                UseBandage(item);
                break;
            default:
                break;
        }
    }

    private void UseBandage(Item item)
    {
        GD.Print($"Предмет использован");
    }
}
