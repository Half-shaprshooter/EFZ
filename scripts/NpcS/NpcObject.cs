public partial class NpcObject : CharacterBody2D
{
	protected int Speed { get; set; }

	[Export] public int SpeedWalk = 200;

	[Export] public int SpeedFast = 350;

	[Export] public int SpeedRun = 600;

	[Export] public float RotationSpeed = 5f;
	[Export] public float BattleRotationSpeed = 15f;

	protected PlayerControl Player;

	protected Vector2 PlayerPosForAngle;
	protected Vector2 TargetPosForAngle;

	protected Vector2 PlayerPosToFollow;
	protected Vector2 TargetPosToFollow;

	protected bool PlayerNear;
	
	[Export] public Host relationToPlayer = Host.Friendly;
	protected HostImpl relation;

	public NpcObject()
	{
		var health = this.GetNodeOrNull<Health>("Health");
		var host = this.GetNodeOrNull<HostImpl>("HostImpl");

		if (health == null)
		{
			host = new HostImpl();
			health = new Health(); // Создаем новый узел Health
			this.AddChild(host);
			host.Name = "HostImpl";
			this.AddChild(health); // Добавляем его к collider
			health.Name = "Health"; // Устанавливаем имя узла
		}
	}

	//Движение со скоростью ходьбы
	public void SetVelocityWalk(Vector2 direction)
	{
		Velocity = direction.Normalized() * SpeedWalk;
	}

	//Движение со скоростью быстрого шага
	public void SetVelocityFast(Vector2 direction)
	{
		Velocity = direction.Normalized() * SpeedFast;
	}

	//Движение со скоростью бега
	public void SetVelocityRun(Vector2 direction)
	{
		Velocity = direction.Normalized() * SpeedRun;
	}

	public void RotateTowardsDirectionSmoothly(Vector2 direction, float delta, float rotationSpeedToUse)
	{
		if (direction.LengthSquared() < 0.001f)
			return;

		float targetAngle = direction.Angle();
		Rotation = Mathf.LerpAngle(Rotation, targetAngle, rotationSpeedToUse * delta);
	}
	
	public void RotateTowardsAngleSmoothly(float targetAngle, float delta, float rotSpeed)
    {
        Rotation = Mathf.LerpAngle(Rotation, targetAngle, delta * rotSpeed);
    }
}