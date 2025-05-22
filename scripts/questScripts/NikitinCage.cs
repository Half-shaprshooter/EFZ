using Godot;
using System;

public partial class NikitinCage : StaticBody2D
{
	private AnimatedSprite2D _animatedSprite2d;
	private bool isActive = true;
	private PodvalSound podvalSound;
	private CollisionPolygon2D doorsNikita;
	private Kletka _kletka;
	private Kletka _kletka1;
	private Kletka _kletka2;
	private Kletka _kletka3;
	private Kletka _kletka4;
	
	
	public override void _Ready()
	{
		_animatedSprite2d = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		podvalSound = GetNode<PodvalSound>("/root/main/Podval/SoundPlayers");
		doorsNikita = GetNode<CollisionPolygon2D>("DoorNikitin");
		
		_kletka = GetNode<Kletka>("/root/main/Podval/Kletka");
		_kletka1 = GetNode<Kletka>("/root/main/Podval/Kletka2");
		_kletka2 = GetNode<Kletka>("/root/main/Podval/Kletka5");
		_kletka3 = GetNode<Kletka>("/root/main/Podval/Kletka6");
		_kletka4 = GetNode<Kletka>("/root/main/Podval/Kletka3");
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
	
	public void _on_trigger_body_entered(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			_kletka.open();
			_kletka1.open();
			_kletka2.open();
			_kletka3.open();
			_kletka4.open();
		}
	}
}
