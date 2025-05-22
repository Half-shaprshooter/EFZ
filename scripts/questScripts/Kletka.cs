using Godot;
using System;

public partial class Kletka : StaticBody2D
{
	private AnimatedSprite2D _animatedSprite2d;
	private bool isActive = true;
	private PodvalSound podvalSound;
	private CollisionPolygon2D door;
	public override void _Ready()
	{
		door = GetNode<CollisionPolygon2D>("Door");
		_animatedSprite2d = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		podvalSound = GetNode<PodvalSound>("/root/main/Podval/SoundPlayers");
	}
	
	public override void _Process(double delta)
	{
		if (isActive)
		{
			_animatedSprite2d.Play("Electric");
		}
		else
		{
			_animatedSprite2d.Stop();
		}
	}

	public void open()
	{
		door.Disabled = true;
	}

	public void toggle()
	{
		podvalSound.stop();
		_animatedSprite2d.Play("Open");
		isActive = false;
	}
}
