using Godot;
using System;

public partial class Kletka : StaticBody2D
{
	private AnimatedSprite2D _animatedSprite2d;
	private bool isActive = true;
	public override void _Ready()
	{
		_animatedSprite2d = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}
	
	public override void _Process(double delta)
	{
		if (isActive)
		{
			_animatedSprite2d.Play();
		}
		else
		{
			_animatedSprite2d.Stop();
		}
	}

	public void toggle()
	{
		isActive = false;
	}
}
