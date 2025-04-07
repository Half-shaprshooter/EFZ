using Godot;
using System;
using EscapeFromZone.scripts.Items;

public partial class EquipmentSlot : Panel
{
	[Signal]
	public delegate void SlotEnteredEventHandler(EquipmentSlot slot);
	
	[Signal]
	public delegate void SlotExitedEventHandler(EquipmentSlot slot);

	public ColorRect Filter { get; set; }
	public int SlotID;
	private bool _isHovering = false;
	
	public enum CategoryType
	{
		Weapon,
		Head,
		Body,
		Accessory,
		None // Значение по умолчанию
	}
	
	[Export]
	public CategoryType SlotCategory { get; set; } = CategoryType.None;
	
	public enum States
	{
		Default,
		Taken,
		Free
	}

	public States State = States.Default;
	public Item ItemStored = null;

	public override void _Ready()
	{
		Filter = GetNode<ColorRect>("StatusFilter");
		SetColor(States.Default);
	}

	public void SetColor(States aState = States.Default)
	{
		switch (aState)
		{
			case States.Default:
				Filter.Color = new Color(Colors.White, 0.0f);
				break;
			case States.Taken:
				Filter.Color = new Color(Colors.Red, 0.2f);
				break;
			case States.Free:
				Filter.Color = new Color(Colors.Green, 0.2f);
				break;
		}
	}

	public override void _Process(double delta)
	{
		if (GetGlobalRect().HasPoint(GetGlobalMousePosition()))
		{
			if (!_isHovering)
			{
				_isHovering = true;
				EmitSignal(SignalName.SlotEntered, this);
			}
		}
		else
		{
			if (_isHovering)
			{
				_isHovering = false;
				EmitSignal(SignalName.SlotExited, this);
			}
		}
	}

	public bool CanAcceptItem(Item item)
	{
		return item.Category == SlotCategory.ToString() && State != States.Taken;
	}
	
}
