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
	
	public float fireRate;
	public float timeUntilFire;

	public EnemyNpc()
	{
		var a = this.GetNode<HostImpl>("HostImpl");
		a._host = Host.Enemy;
		GD.Print("Enemy is: " + a._host);
	}
	
	protected void TimerManager()
	{
		if (CastTimer <= 0f) AbleToCast = true; 
		else CastTimer -= (float)GetProcessDeltaTime();
		if (CastDuration <= 0f) IsAttacking = true; 
		else CastDuration -= (float)GetProcessDeltaTime();
	}
}
