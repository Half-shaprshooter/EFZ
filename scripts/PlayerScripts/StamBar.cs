using Godot;
using System;

public partial class StamBar : ProgressBar
{
    double _stamina;

    public override void _Ready()
	{
		_stamina = PlayerData.PlayerStamina;
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
        _stamina = PlayerData.PlayerStamina;
        Value = _stamina;
    }
}
