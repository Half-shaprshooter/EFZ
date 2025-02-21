using Godot;
using System;

public partial class Quest : StaticBody2D
{
	public static Vector2 globalPos;
	public override void _Ready()
	{
		globalPos = this.GlobalPosition;
	}
	
	public override void _Process(double delta)
	{
		globalPos = this.GlobalPosition;
	}
}
