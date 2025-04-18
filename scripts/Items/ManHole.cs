using Godot;
using System;

public partial class ManHole : Sprite2D
{
	private bool _isInArea = false;
	private AnimationPlayer _playerBlack;
	private AnimationPlayer _playerWhite;
	
	private bool once = true;
	public override void _Ready()
	{
		_playerBlack = GetNode<AnimationPlayer>("Black");
		_playerWhite = GetNode<AnimationPlayer>("White");
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("open_door") && _isInArea && once)
		{
			_playerBlack.Play("Black");
			once = false;
		}
	}

	public void OnPnimationPlayerAnimationFinished(String name)
	{
		
		var targetNode = GetNode("/root/main/Player");
		targetNode.Call("Transport");
		_playerWhite.Play("White");
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
