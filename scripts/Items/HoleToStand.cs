using Godot;
using System;

public partial class HoleToStand : Sprite2D
{
	private bool _isInArea = false;
	private AnimationPlayer _playerBlack;
	private AnimationPlayer _playerWhite;
	[Export] string transportNumber = "";
	private AudioStreamPlayer2D audioPlayer;
  
	//private AudioStream MANHOLE_OPEN = (AudioStream)GD.Load("res://sounds/effects/ManHoleOpening.mp3");
	private AudioStream STAIRS_SOUND = (AudioStream)GD.Load("res://sounds/effects/UpDownStairs.mp3");
  
  
	private bool once = true;
	public override void _Ready()
	{
		audioPlayer = GetNode<AudioStreamPlayer2D>("EffectPlayer");
		_playerBlack = GetNode<AnimationPlayer>("Black");
		_playerWhite = GetNode<AnimationPlayer>("White");
	}
  
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("open_door") && _isInArea && once)
		{
			//audioPlayer.Stream = STAIRS_SOUND;
			//audioPlayer.Play();
      
			_playerBlack.Play("Black");
			once = false;
		}
	}

	public void OnPnimationPlayerAnimationFinished(String name)
	{
		var targetNode = GetNode("/root/main/Player");
		targetNode.Call("Transport" + transportNumber);
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
