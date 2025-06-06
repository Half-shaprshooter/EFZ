public partial class Wolf: EnemyNpc
{
	public override void _Ready()
	{
		Speed = 60;
		DamageFromAttack = 10;
		fireRate = 1 / 1;
	}
	
	public override void _Process(double delta)
	{
		PlayerPosForAngle = PlayerControl.globalPos;
		PlayerPosToFollow = PlayerControl.localPos;
		
		TargetPosForAngle = (PlayerPosForAngle - Position).Normalized();
		TargetPosToFollow = (PlayerPosToFollow - Position).Normalized();
		
		var targetRotation = (PlayerPosForAngle - GlobalPosition).Angle();
		Rotation = (float)Mathf.LerpAngle(Rotation, targetRotation, 10 * delta);
		
		if (Position.DistanceTo(PlayerPosToFollow) < (float)500)
		{
			Position += (TargetPosToFollow * Speed * (float)GetProcessDeltaTime());
			MoveAndSlide();
		}
		if (PlayerNear && AbleToCast)
		{
			JumpAttack();
			IsAttacking = false;
			CastTimer = CastTimerReset;
			CastDuration = CastDurationReset;
		}
		if(IsAttacking)
		{
			Speed = 60;
		}

		TimerManager();
		if (InMeleeRange && timeUntilFire > fireRate)
		{
			AbleToCast = false;
			Player.TakeDmg(DamageFromAttack);
			timeUntilFire = 0f;
		}
		else
		{
			timeUntilFire += (float)delta;
		}
	}

	private void OnDetectionAreaBodyEntered(Node2D body)
	{
		if (body is PlayerControl)
		{
			Player = body as PlayerControl;
			PlayerNear = true;
		}
	}

	private void OnDetectionAreaBodyExited(Node2D body)
	{
		PlayerNear = false;
	}

	private void OnDamageAreaBodyEntered(Node2D body)
	{
		if (body.HasMethod("Player"))
		{
			InMeleeRange = true;
		}
	}
	private void OnDamageAreaBodyExited(Node2D body)
	{
		if (body.HasMethod("Player"))
		{
			InMeleeRange = false;
		}
	}

	private void JumpAttack()
	{
		Speed = 500;
	}
}
