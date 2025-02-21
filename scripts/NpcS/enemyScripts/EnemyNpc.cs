public partial class EnemyNpc : NpcObject
{
	protected bool AbleToCast = true;
	protected bool IsAttacking = true;
	protected bool InMeleeRange;
	
	protected float CastTimer = 5f;
	protected float CastTimerReset = 5f;
	protected float CastDuration = 0.3f;
	protected float CastDurationReset = 0.3f;
	
	protected int DamageFromAttack;

	protected void TimerManager()
	{
		if (CastTimer <= 0f) AbleToCast = true; 
		else CastTimer -= (float)GetProcessDeltaTime();
		if (CastDuration <= 0f) IsAttacking = true; 
		else CastDuration -= (float)GetProcessDeltaTime();

	}
	public override void _Ready()
	{
		Hostility = Hostility.Enemy;
		Health = 50;
	}
}
