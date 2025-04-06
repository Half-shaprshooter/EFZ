namespace EscapeFromZone.scripts.Items;

public partial class Item : Node2D
{
	public TextureRect IconRectPath { get; set; }

	public int ItemID;
	private List<Vector2I> OriginItemGrids = new();
	public List<Vector2I> ItemGrids = new();
	public bool Selected = false;
	public object GridAnchor = null;
	public string Category { get; private set; }

	public override void _Ready()
	{
		IconRectPath = GetNode<TextureRect>("Icon");
	}

	public override void _Process(double delta)
	{
		if (Selected)
		{
			GlobalPosition = GlobalPosition.Lerp(GetViewport().GetMousePosition(), (float)(25 * delta));
		}
	}

	/// <summary>
	/// Загружает и настраивает предмет по его ID.
	/// </summary>
	/// <param name="aItemID">ID загружаемого предмета</param>
	public void LoadItem(int aItemID)
	{
		ItemID = aItemID;
		var iconPath = $"res://sprites/{DataHandler.itemData[aItemID.ToString()]["Name"]}.png";
		IconRectPath.Texture = GD.Load<Texture2D>(iconPath);

		Category = DataHandler.itemData[aItemID.ToString()]["Category"].ToString();
		
		foreach (var grid in DataHandler.itemGridData[aItemID.ToString()])
		{
			Vector2I convertedGrid = new(int.Parse(grid[0]), int.Parse(grid[1]));
			ItemGrids.Add(convertedGrid);
		}

		OriginItemGrids = ItemGrids;
	}

	public string GetItemName(int aItemID)
	{
		return DataHandler.itemData[aItemID.ToString()]["Name"].ToString();
	}

	/// <summary>
	/// Поворачивает предмет и его занимаемые ячейки на 90°.
	/// </summary>
	public void RotateItem()
	{
		for (var i = 0; i < ItemGrids.Count; i++)
		{
			var tempY = ItemGrids[i].X;
			ItemGrids[i] = new Vector2I(-ItemGrids[i].Y, tempY);
		}
		
		RotationDegrees += 90;
		if (RotationDegrees >= 360)
		{
			RotationDegrees = 0;
		}
	}

	public void ResetAngle()
	{
		RotationDegrees = 0;
		ItemGrids = OriginItemGrids;
	}

	/// <summary>
	///  Перемещает предмет в указанную позицию.
	/// </summary>
	/// <param name="destination"></param>
	public void SnapTo(Vector2 destination)
	{
		var tween = GetTree().CreateTween();
		
		if ((int)RotationDegrees % 180 == 0)
		{
			destination += IconRectPath.Size / 2;
		}
		else
		{
			Vector2 tempXY = new(IconRectPath.Size.Y, IconRectPath.Size.X);
			destination += tempXY / 2;
		}
		
		tween.TweenProperty(this, "global_position", destination, 0.15).SetTrans(Tween.TransitionType.Sine);
		Selected = false;
	}
}
