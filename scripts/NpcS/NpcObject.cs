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

	[Export] protected string relationToPlayer = "neutral";
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
	protected void MoveInDirectionWalk(Vector2 direction)
	{
		Velocity = direction.Normalized() * SpeedWalk;
		MoveAndSlide();
	}
	
	//Движение со скоростью быстрого шага
	protected void MoveInDirectionFast(Vector2 direction)
	{
		Velocity = direction.Normalized() * SpeedFast;
		MoveAndSlide();
	}

	//Движение со скоростью бега
	protected void MoveInDirectionRun(Vector2 direction)
	{
		Velocity = direction.Normalized() * SpeedFast;
		MoveAndSlide();
	}

	protected void RotateTowardsDirectionSmoothly(Vector2 direction, CharacterBody2D body, float delta, float RotationSpeed)
	{
		if (direction.Length() < 1f || body == null)
			return;

		float targetAngle = direction.Angle();
		float currentAngle = body.Rotation;
		body.Rotation = Mathf.LerpAngle(currentAngle, targetAngle, RotationSpeed * delta);
	}
}