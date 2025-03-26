using Godot;
using System;

public partial class HpBar : ProgressBar
{
    int _health;

    public override void _Ready()
	{
		_health = PlayerData.PlayerHealth;
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
        _health = PlayerData.PlayerHealth;
        Value = _health;
    }
}
