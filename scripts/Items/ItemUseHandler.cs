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
            case 10: // Ключ
                return true;
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
        if (QuestList.Instance.HaveQuest("Вылечи ногу"))
        {
            QuestList.Instance.RemoveQuest("Вылечи ногу");
        }
        return true;
    }
}
