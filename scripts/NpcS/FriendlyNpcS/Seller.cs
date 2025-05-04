using Godot;
using System;

public partial class Seller : TalkableNpc
{
	private Label _label;
	private static List<NpcDialogue> _npcDialogues;
	private Inventory _sellerInventory;
	private TradeManager _tradeManager;

	public override void _Ready()
	{
		NpcName = "SellerName";
		InterfaceSelectionObject interSelect = new InterfaceSelectionObject(1, "Торговать");
		InterfaceSelectionObject interSelect2 = new InterfaceSelectionObject(2, "Уйти");
		InterfaceSelectionObject interSelect4 = new InterfaceSelectionObject(3, "qweqeqwe");
		InterfaceSelectionObject interSelect3 = new InterfaceSelectionObject(-1, "Ок");
		_npcDialogues = new List<NpcDialogue>
		{
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect, interSelect2, interSelect4}, "TEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST TEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTBOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXTTEST BOTTOM TEXT", 0),
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect3}, "Вариант текста 2", 1),
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect3}, "Вариант текста 3", 2)
		};
		
		_label = GetNode<Label>("ButtonText");
		_sellerInventory = GetNode<Inventory>("UI/SellerInventory");
		_tradeManager = GetNode<TradeManager>("/root/TradeManager");
		
	}
	
	public override void _Process(double delta)
	{
		if (DialogueManager.IsTradeIndex)
		{
			DialogueManager.IsTradeIndex = false;
			_tradeManager.StartTraiding(_sellerInventory);
		}
	}
	
	public void SetDialog()
	{
		InterfaceManager.dialogueManager.NpcDialogues = _npcDialogues;
		InterfaceManager.dialogueManager.DialogHeader = NpcName;
		GetTree().Paused = true;
	}

	private void OnDetectionAreaBodyEntered(Node2D body)
	{
		if (body is PlayerControl)
		{
			_label.Visible = true;
			PlayerNear = true;
		}
	}

	private void OnDetectionAreaBodyExited(Node2D body)
	{
		if (body is PlayerControl)
		{
			_label.Visible = false;
			PlayerNear = false;
		}
	}
}
