using Godot;
using System;

public partial class DoorTrigger : Sprite2D
{
	private bool _isInArea = false;
	private AnimationPlayer _playerBlack;
	private AnimationPlayer _playerWhite;
	private Sprite2D _corpse;
	private Node2D _note;
	private Node2D _key;
	private Node2D _bandage;
	private CharacterBody2D _seller;
	private AudioStreamPlayer2D audioPlayer;
	
	//private AudioStream MANHOLE_OPEN = (AudioStream)GD.Load("res://sounds/effects/ManHoleOpening.mp3");
	//private AudioStream STAIRS_SOUND = (AudioStream)GD.Load("res://sounds/effects/UpDownStairs.mp3");
	
	
	private bool once = true;
	public override void _Ready()
	{
		_corpse = GetNode<Sprite2D>("/root/main/NavigationRegion2D/House6/Сorpse");
		_note = GetNode<Node2D>("/root/main/NavigationRegion2D/House6/SellerNote");
		_key = GetNode<Node2D>("/root/main/NavigationRegion2D/House6/KeyItem");
		_bandage = GetNode<Node2D>("/root/main/NavigationRegion2D/House6/Bandage");
		_seller = GetNode<CharacterBody2D>("/root/main/Seller");
		//audioPlayer = GetNode<AudioStreamPlayer2D>("EffectPlayer");
		_playerBlack = GetNode<AnimationPlayer>("Black");
		_playerWhite = GetNode<AnimationPlayer>("White");
	}
	
	public override void _Process(double delta)
	{
		if (_isInArea && once)
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
		_playerWhite.Play("White");
		_corpse.Visible = true;
		_note.Visible = true;
		_key.Visible = true;
		_bandage.Visible = true;
		PlayerData.PlayerHealth = 20;
		_seller.QueueFree();
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
