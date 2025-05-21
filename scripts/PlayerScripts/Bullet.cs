using Godot;
using System;

public partial class Bullet : RigidBody2D
{
	[Export] public float damage = 1000f;
	[Export] public float speed = 1000f;
	[Export] public Node2D whoAttacks = null;

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
			GD.Print(collider.GetGroups());
			if (collider != null && collider.IsInGroup("Alive"))
			{
				var health = collider.GetNodeOrNull<Health>("Health");
				var host = collider.GetNodeOrNull<HostImpl>("HostImpl");
				GD.Print(collider + " is before " + host._host);
				host._host = Host.Enemy;
				GD.Print(collider + " is after " + host._host);
				health?.Damage(damage, whoAttacks);
			}
			GD.Print(collider);
			if (collider.IsInGroup("Distructable"))
			{
				GD.Print("Damage From Bullet");
				var health = collider.GetNodeOrNull<Health>("Health");
				GD.Print(collider);
				GD.Print(health);
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
