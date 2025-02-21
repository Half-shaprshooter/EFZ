using Godot;
using System;

public partial class Bullet : RigidBody2D
{
	[Export] public float damage = 1000f;
	[Export] public float speed = 1000f; 

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Transform.X * speed * (float)delta;
		Vector2 newPosition = GlobalPosition + velocity;
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, newPosition);
		var result = spaceState.IntersectRay(query);

		if (result != null && result.ContainsKey("collider"))
		{
			var collider = (Node)result["collider"];
			if (collider != null && collider.IsInGroup("Alive"))
			{
				GD.Print("Take Dmg: " + damage);
				var health = collider.GetNodeOrNull<Health>("Health");
				health?.Damage(damage);
			}
			GlobalPosition = (Vector2)result["position"];
			QueueFree();
		}
		else
		{
			GlobalPosition = newPosition;
		}
	}
}
