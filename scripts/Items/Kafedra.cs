using Godot;
using System;

public partial class Kafedra : Sprite2D
{
	private bool _isInArea = false;
	private AnimationPlayer player;
	private bool once = true;
	
	public override void _Ready()
	{
		player = GetNode<AnimationPlayer>("AnimationPlayer");
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("open_door") && _isInArea && once)
		{
			player.Play("Slide");
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
