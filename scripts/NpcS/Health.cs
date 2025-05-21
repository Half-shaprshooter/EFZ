using Godot;
using System;

public partial class Health : Node2D
{
	// Сигнал, который будет испускаться при получении урона.
    // Передает количество урона и ссылку на узел атакующего.
    [Signal]
    public delegate void DamagedEventHandler(float damageAmount, Node attacker);

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

	public void Damage(float damage, Node2D attacker = null)
	{
		health -= damage;
		GD.Print("Dmg in Health class is: " + damage);

		// Испускаем сигнал с информацией об уроне и атакующем
        EmitSignal(SignalName.Damaged, damage, attacker);

		if (health < 0)
		{
			GD.Print("Dead");
			GetParent().QueueFree();
		}
	}
}
 