namespace EscapeFromZone.scripts.enemyScripts;

public partial class HumanEnemyNpc : EnemyNpc
{
	private Node2D playerBody;
	private bool isHeSee;
	private RayCast2D lookRay;
	private Area2D proximityArea;
	[Export] CharacterBody2D body;

	public EnemyGun gun;
	
	public override void _Ready()
	{
		gun = GetNode<EnemyGun>("Gun");
		lookRay = GetNode<RayCast2D>("LookRay");
		proximityArea = GetNode<Area2D>("ProximityArea");
		proximityArea.Visible = true;
		proximityArea.BodyEntered += _on_proximity_area_body_entered;
		proximityArea.BodyExited += _on_proximity_area_body_exited;
	}

	public void _on_proximity_area_body_entered(Node2D body)
	{
		if (body is PlayerControl)
		{
			playerBody = body;
			gun.makeNear(true);
		}
	}

	public void _on_proximity_area_body_exited(Node2D body)
	{
		if (body is PlayerControl)
		{
			playerBody = null;
			gun.makeNear(false);
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		if (playerBody != null)
		{
			BuildRayToTarget(playerBody, delta);
		}
		proximityArea.Rotation = body.Rotation;
	}
	
	public override void _Process(double delta)
	{
		
	}
	private void BuildRayToTarget(Node2D target, double delta)
	{
		lookRay.Rotation = body.Rotation;
		lookRay.Position = Vector2.Zero;
		lookRay.GlobalPosition = body.GlobalPosition;
		
		PlayerPosForAngle = PlayerControl.globalPos;
		PlayerPosToFollow = PlayerControl.localPos;
		
		var targetRotation = (PlayerPosForAngle - GlobalPosition).Angle();
		Rotation = (float)Mathf.LerpAngle(Rotation, targetRotation, 10 * delta);
		
		Vector2 localTarget = lookRay.ToLocal(target.GlobalPosition);
		lookRay.TargetPosition = localTarget;
		
		var targetNode = GetNode("/root/main/Player");
		
		if (lookRay.IsColliding() && lookRay.GetCollider() == target)
		{
			gun.makeNear(true);
		}
		else
		{
			gun.makeNear(false);
		}
	}
}
