using Godot;
using System;
using EscapeFromZone.scripts.Items;

public partial class ItemUseHandler : Control
{

    public bool UseItem(Item item)
    {
        switch (item.ItemID)
        {
            case 9: // Бинт
                return UseBandage(item);
            default:
                return false;
        }
    }

    private bool UseBandage(Item item)
    {
        var hpIncreaseAmount = PlayerData.PlayerMaxHealth - PlayerData.PlayerHealth >= 20
            ? 20
            : PlayerData.PlayerMaxHealth - PlayerData.PlayerHealth;
        if (hpIncreaseAmount == 0)
            return false;
        PlayerData.PlayerHealth += hpIncreaseAmount;
        
        GD.Print($"Предмет использован");
        return true;
    }
}
