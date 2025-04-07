using Godot;
using System;

public partial class PlayerData : Node
{
	public static int PlayerHealth = 100;

	public static int PlayerMaxHealth = 100;

	public static int staminaConsumption = 10;
	public static double PlayerStamina = 100;
	public static int PlayerMaxStamina = 100;

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
