namespace EscapeFromZone.scripts.enemyScripts;

using Godot;

public partial class EnemyBullet : RigidBody2D
{
	[Export] public float damage = 1000f;
	[Export] public float speed = 1000f;
	private int dmg = 30;

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
			
			//TODO: Плохая реализация, потом через Export нужно все переделать, где есть привязка к сцене
			if (collider != null && collider.IsInGroup("Player"))
			{
				var targetNode = GetNode("/root/main/Player");
				targetNode.Call("TakeDmg", 30);
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
