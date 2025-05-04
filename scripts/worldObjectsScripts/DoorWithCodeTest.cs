public partial class DoorWithCodeTest : StaticBody2D
{
	[Export] private bool _isInArea;

	private bool _isOpen; 

    //ввод нужного нода замка в редакторе
    [Export] private CodeLock _codeLock;
    private bool _islocked;
	private bool isUnlocking = false;

	//canvas в котором надпись: "[E] для ввода пинкода"
    private CanvasLayer _label_E_ForCode;
	
	public override void _Ready()
	{
		Rotation = HousesObjectsData.rotation;
		_isOpen = HousesObjectsData.isOpen;

        _islocked = _codeLock.isLocked;

        _label_E_ForCode = GetNode<CanvasLayer>("Canvas");
        _label_E_ForCode.Visible = false;
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
        _islocked = _codeLock.isLocked;

		//если мы в зоне двери, она закрыта и не открывается сейчас, то появляется надпись, в ином случае её нет
        if (_isInArea && _islocked && !isUnlocking)
        {
            _label_E_ForCode.Visible = true;
        }

        else
        {
            _label_E_ForCode.Visible = false;
        }
		
		//если мы пытаемся открыть дверь, находимся в зоне и она закрыта, то у нас появится кодовый замок
		if (Input.IsActionJustPressed("open_door") && _isInArea && _islocked)
		{
			isUnlocking = true;
			_codeLock.StartPinCode();
		}

        //если мы не в зоне, то прогресс ввода кода сбрасывается
		if (!_isInArea)
		{	
			isUnlocking = false;
			_codeLock.CancelPinCode();
		}

		//если дверь не заперта, мы в зоне и нажимаем кнопку открыть дверь, то она меняет своё состояние
        if (Input.IsActionJustPressed("open_door") && _isInArea && !_islocked)
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
