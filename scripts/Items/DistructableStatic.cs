namespace EscapeFromZone.scripts.Items;

public partial class DistructableStatic : StaticBody2D
{
	private Health health;

	[Export]
	public PackedScene[] Scenes { get; set; } = Array.Empty<PackedScene>();
	
	[Export]
	public int ItemId { get; set; }
	
	[Export]
	public string ItemName { get; set; }
	
	[Export]
	public Texture2D ItemTexture { get; set; } = GD.Load<Texture2D>("res://sprites/AK47.png");
	
	private Control _inventoryUI;
	
	public DistructableStatic()
	{
		health = this.GetNodeOrNull<Health>("Health");
		
		if (health == null)
		{
			health = new Health(2L);
			this.AddChild(health);
			health.Name = "Health"; 
		}
		GD.Print();
	}
	public override void _Ready()
	{
		AddToGroup("Distructable");
	}
	
	public override void _Process(double delta)
	{
		if (this.health.health < 0)
		{
			SpawnObjects();
		}
	}

	public void SpawnObjects()
	{
		if (Scenes != null && Scenes.Length != 0)
		{
			foreach (var scene in Scenes)
			{
				var item = (FloorItem)scene.Instantiate();
				item.ItemId = ItemId;
				item.ItemName = ItemName;
				item.ItemTexture = ItemTexture;
				item.GlobalPosition = GlobalPosition;
				GetTree().Root.AddChild(item);
			}
		}
	}
}
