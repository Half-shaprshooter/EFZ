public partial class NpcObject : CharacterBody2D
{
	protected Hostility Hostility { get; set; }
	protected int Health { get; set; }
	protected int Speed { get; set; }
	
	protected PlayerControl Player;
	
	protected Vector2 PlayerPosForAngle;
	protected Vector2 TargetPosForAngle;
	
	protected Vector2 PlayerPosToFollow;
	protected Vector2 TargetPosToFollow;
	
	protected bool PlayerNear;
	
	public override void _Ready()
	{
		
	}
	public override void _Process(double delta)
	{
		
	}
	
	protected void TakeDmg(int dmg)
	{
		Health -= dmg;
	}
	protected void Heal(int heal)
	{
		Health += heal;
	}
}
