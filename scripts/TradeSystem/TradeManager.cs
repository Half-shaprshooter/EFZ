using Godot;
using System;
using System.Diagnostics;
using EscapeFromZone.scripts.Items;

public partial class TradeManager : Node
{
    
    [Signal]
    public delegate void TradeStartedEventHandler();
    [Signal]
    public delegate void TradeEndedEventHandler();

    private Inventory _playerInventory;
    private Inventory _sellerInventory;
    private bool _isTrading = false;

    public void GetPlayerInventory(Inventory inventory)
    {
        _playerInventory = inventory;
    }

    public override void _Process(double delta)
    {
        if (_isTrading && Input.IsKeyPressed(Key.G))
        {
            StopTrading();
        }
    }

    public void StartTraiding(Inventory sellerInventory)
    {
        if(_isTrading)
            return;
        
        _sellerInventory = sellerInventory;
        _isTrading = true;
        
        _playerInventory.Visible = true;
        _sellerInventory.Visible = true;

        _playerInventory.IsTradeMode = true;
        _sellerInventory.IsTradeMode = true;

        EmitSignal(SignalName.TradeStarted);
    }

    public void StopTrading()
    {
        if(!_isTrading)
            return;
        _isTrading = false;
        _playerInventory.Visible = false;
        _sellerInventory.Visible = false;
        _playerInventory.IsTradeMode = false;
        _sellerInventory.IsTradeMode = false;
        _sellerInventory = null;

        EmitSignal(SignalName.TradeEnded);
    }

    public void TryMakeItemFromPlayerTransfer(Item item)
    {
        _sellerInventory.AddItem(item.ItemID);
        
        
    }

    public void TryMakeItemFromSellerTransfer(Item item)
    {
        _playerInventory.AddItem(item.ItemID);
    }
}
