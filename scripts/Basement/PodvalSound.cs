using Godot;
using System;

public partial class PodvalSound : Node2D
{
	private AudioStreamPlayer2D firstPlayer;
	private AudioStreamPlayer2D secondPlayer;
	public override void _Ready()
	{
		firstPlayer = GetNode<AudioStreamPlayer2D>("FirstPlayer");
		secondPlayer = GetNode<AudioStreamPlayer2D>("SecondPlayer");
		firstPlayer.Play();
	}
	public override void _Process(double delta)
	{
	}

	public void stop()
	{
		secondPlayer.Play();
	}
}
