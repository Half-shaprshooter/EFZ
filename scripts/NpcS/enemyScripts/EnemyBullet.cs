namespace EscapeFromZone.scripts.enemyScripts;

using Godot;

public partial class EnemyBullet : RigidBody2D
{
    [Export] public float damage = 1000f;
    [Export] public float speed = 1000f;
    private bool _hasHit = false;

    public override void _Ready()
    {
        LinearVelocity = Transform.X * speed;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (_hasHit)
            return;

        Vector2 from = GlobalPosition;
        Vector2 to = from + LinearVelocity * state.Step;

        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(from, to);
        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

        var result = spaceState.IntersectRay(query);

        if (result != null && result.ContainsKey("collider"))
        {
            _hasHit = true;

            var collider = result["collider"].As<Node>();
            if (collider != null && collider.IsInGroup("Player"))
            {
                collider.Call("TakeDmg", 30);
            }

            GlobalPosition = (Vector2)result["position"];
            QueueFree();
        }
    }
}

