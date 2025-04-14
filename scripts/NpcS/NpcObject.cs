public partial class NpcObject : CharacterBody2D
{
	protected int Speed { get; set; }

	protected PlayerControl Player;

	protected Vector2 PlayerPosForAngle;
	protected Vector2 TargetPosForAngle;

	protected Vector2 PlayerPosToFollow;
	protected Vector2 TargetPosToFollow;

	protected bool PlayerNear;

	[Export] protected string relationToPlayer = "neutral";
	
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
}