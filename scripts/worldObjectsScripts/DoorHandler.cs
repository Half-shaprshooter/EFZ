public partial class DoorHandler : StaticBody2D
{
	private bool _isInArea;

	private AudioStreamPlayer2D effect;
	private AudioStream DOOR_SOUND= (AudioStream)GD.Load("res://sounds/effects/DoorOpening.mp3");
	private AnimationPlayer _animationPlayer;

	private bool _isOpen; 
	
	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_isOpen = HousesObjectsData.isOpen;
		effect = GetNode<AudioStreamPlayer2D>("DoorEffect");
	}

	private void OnDoorEventBodyEntered(Node body)
	{
		if (body.HasMethod("Player"))
		{
			InArea();
		}
	}

	private void OnDoorEventBodyExited(Node body)
	{
		if (body.HasMethod("Player"))
		{
			OutArea();
		}
	}

	private void InArea()
	{
		_isInArea = true;
	}
	
	private void OutArea()
	{
		_isInArea = false;
	}

	public override void _Process(double delta)
	{
		HousesObjectsData.isOpen = _isOpen;
		
		if (Input.IsActionJustPressed("open_door") && _isInArea)
		{
			ToggleDoor();
			effect.Stream = DOOR_SOUND;
			effect.Play();
		}
	}

	private void ToggleDoor()
	{
		_isOpen = !_isOpen;

		if (_isOpen)
		{
			_animationPlayer.Play("door_open");
		}
		else
		{
			_animationPlayer.Play("door_closed");
		}
	}
}
