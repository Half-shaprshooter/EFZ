using Godot;
using System;

public partial class NikitinCage : StaticBody2D
{
	private AnimatedSprite2D _animatedSprite2d;
	private bool isActive = true;
	private PodvalSound podvalSound;
	private CollisionPolygon2D doorsNikita;
	public override void _Ready()
	{
		_animatedSprite2d = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		podvalSound = GetNode<PodvalSound>("/root/main/Podval/SoundPlayers");
		doorsNikita = GetNode<CollisionPolygon2D>("DoorNikitin");
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

	public void toggle()
	{
		podvalSound.stop();
		doorsNikita.Disabled = true;
		_animatedSprite2d.Play("Open");
		isActive = false;
	}
}
