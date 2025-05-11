using Godot;
using System;

public partial class Kafedra : Sprite2D
{
	private bool _isInArea = false;
	private AnimationPlayer player;
	private bool once = true;

	private AudioStreamPlayer2D audioPlayer;

	private AudioStream KAFEDRA_SOUND = (AudioStream)GD.Load("res://sounds/effects/KafedraMoving.mp3");
	
	public override void _Ready()
	{
		audioPlayer = GetNode<AudioStreamPlayer2D>("EffectsPlayer");
		player = GetNode<AnimationPlayer>("AnimationPlayer");
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("open_door") && _isInArea && once)
		{
			audioPlayer.Stream = KAFEDRA_SOUND;
			audioPlayer.Play();
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
