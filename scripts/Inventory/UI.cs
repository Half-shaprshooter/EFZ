using Godot;
using System;

public partial class UI : CanvasLayer
{
	[Export]
	private Node UIInventory;

	// Called when the node enters the scene tree for the first time
	public override void _Ready()
	{
		UIInventory = GetNode<Node>(".");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame
	public override void _Process(double delta)
	{
	}
}
