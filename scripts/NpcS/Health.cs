using Godot;
using System;

public partial class Health : Node2D
{
	public float maxHealth = 100f;
	public float health;

	public Health(long count)
	{
		health = count;
	}

	public Health()
	{
		health = maxHealth;
	}

	public void Damage(float damage)
	{
		health -= damage;
		GD.Print("Dmg in Health class is: " + damage);
		if (health < 0)
		{
			GD.Print("Dead");
			GetParent().QueueFree();
		}
	}
}
