namespace EscapeFromZone.scripts.Items;

[Tool] // Позволяет работать скрипту в редакторе Godot
public partial class FloorItem : Node2D
{
	private bool _playerInArea = false;
	
	[Export]
	public int ItemId { get; set; }
	
	[Export]
	public string ItemName { get; set; }
	
	[Export]
	public Texture2D ItemTexture { get; set; } = GD.Load<Texture2D>("res://sprites/AK47.png");

	private string _scenePath = "res://Entities/floorItem.tscn";
	
	private Control _inventoryUI;
	private Sprite2D _iconSprite;
	private Control _interactUI;

	private StaticBody2D partOfProxy;

	public override void _Ready()
	{
		_inventoryUI = GetNode<Control>("/root/main2/Player/UI/Inventory");
		_iconSprite = GetNode<Sprite2D>("Sprite2D");
		_interactUI = GetNode<Control>("interactItem");

		_interactUI.Visible = false;
		
		if (!Engine.IsEditorHint())
		{
			_iconSprite.Texture = ItemTexture;
		}
	}

	public void SetItemId(int newItemId)
	{
		ItemId = newItemId;
	}
	
	public void SetItemTexture(Texture2D texture)
	{
		ItemTexture = texture;
	}

	/// <summary>
	///  Обработка входа игрока в зону предмета
	/// </summary>
	/// <param name="body">тело вхожденного объекта</param>
	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			_interactUI.Visible = true;
			_playerInArea = true;
		}
	}

	/// <summary>
	///  Обработка выхода игрока из зону предмета
	/// </summary>
	/// <param name="body">тело вхожденного объекта</param>
	private void OnBodyExited(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			_interactUI.Visible = false;
			_playerInArea = false;
		}
	}
	
	/// <summary>
	/// Обработка нажатия кнопки подбора предмета и вызов метода инвенторя
	/// </summary>
	/// <param name="delta"></param>
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			_iconSprite.Texture = ItemTexture;
		}
	
		if (_playerInArea && Input.IsActionJustPressed("interact"))
		{
			_inventoryUI.Call("AddItem", ItemId);
			_interactUI.Visible = true;
			QueueFree();
			GD.Print("Произошло действие");
		}
	}
}
