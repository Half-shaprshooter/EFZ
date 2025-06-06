using Godot;
using System;

public partial class HoleToStand : Sprite2D
{
	private bool _isInArea = false;
	
	private bool once = true;
	public override void _Ready()
	{
		
	}
	
	public override void _Process(double delta)
	{
		if (_isInArea && once)
		{
			var targetNode = GetNode("/root/main/Player");
			targetNode.Call("Transport2");
			once = false;
		}
	}
	
	public void OnAreaBodyEntered(Node body)
	{ 
		if (body.HasMethod("Player"))
		{
			_isInArea = true;
		}
	}

	public void OnAreaBodyExited(Node body)
	{
		if (body.HasMethod("Player"))
		{
			_isInArea = false;
		}
	}
}
