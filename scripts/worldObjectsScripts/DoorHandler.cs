public partial class DoorHandler : StaticBody2D
{
	private bool _isInArea;

	private bool _isOpen; 
	
	public override void _Ready()
	{
		Rotation = HousesObjectsData.rotation;
		_isOpen = HousesObjectsData.isOpen;
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
		HousesObjectsData.rotation = Rotation;
		HousesObjectsData.isOpen = _isOpen;
		
		if (Input.IsActionJustPressed("open_door") && _isInArea)
		{
			ToggleDoor();
		}
	}

	private void ToggleDoor()
	{
		_isOpen = !_isOpen;

		if (_isOpen)
		{
			Rotation -=1.95f;
		}
		else
		{
			Rotation += 1.95f;
		}
	}
}
