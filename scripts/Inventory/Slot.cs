using Godot;
using System;

public partial class Slot : TextureRect
{
	// Объявление сигнала, вызываемого при входе курсора в слот
	[Signal]
	public delegate void SlotEnteredEventHandler(Slot slot);
	
	// Объявление сигнала, вызываемого при выходе курсора из слота
	[Signal]
	public delegate void SlotExitedEventHandler(Slot slot);
	
	public ColorRect Filter { get; set; }

	public int SlotID;
	private bool _isHovering = false;

	public enum States
	{
		Default,
		Taken,
		Free
	}

	public States State = States.Default;
	public object ItemStored = null;

	public override void _Ready()
	{
		Filter = GetNode<ColorRect>("StatusFilter");
	}

	/// <summary>
	/// Устанавливает цвет фильтра в зависимости от состояния слота
	/// </summary>
	/// <param name="aState">Состояние слота</param>
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
}
