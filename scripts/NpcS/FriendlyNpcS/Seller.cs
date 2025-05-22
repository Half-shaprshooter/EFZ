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
		InterfaceSelectionObject interSelect4 = new InterfaceSelectionObject(3, "…");
		InterfaceSelectionObject interSelect5 = new InterfaceSelectionObject(4, "Ага");
		InterfaceSelectionObject interSelect6 = new InterfaceSelectionObject(5, "Ага");
		
		InterfaceSelectionObject interSelect3 = new InterfaceSelectionObject(-1, "Ок");
		_npcDialogues = new List<NpcDialogue>
		{
			new NpcDialogue(
				new List<InterfaceSelectionObject>(){interSelect, interSelect2, interSelect4}, 
				"Ты, значит, от него? Ну, значит, не самый надёжный у тебя выбор друзей.", 0, true),
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect3}, 
				"Вариант текста 2", 1, false),
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect3}, 
				"Вариант текста 3", 2, true),
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect5}, 
				"Ну-ну, смотри-ка. Подходи, коль живой добрался.", 3, true),
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect3}, 
				"Вот это уже лучше. Слушай тогда сюда. Есть тут один сталкер, " +
				"Митя.\n С ним и потолкуй. У него дельце завалялось. Простенькое, но для новенького — в самый раз.\n " +
				"А если справишься... глядишь, и расскажу.\n И помни: тут каждый шаг записывается. " +
				"Даже если ты сам этого не замечаешь.\n.", 4, true),
			
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

	//добавлен async, чтобы работал показ текста
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
