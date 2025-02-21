public partial class SomeFriendlyNpc : TalkableNpc
{
	public override void _Ready()
	{
	}
	
	public override void _Process(double delta)
	{
		if (Hostility.Equals(Hostility.Enemy))
		{
			AttackPlayer();
		}
	}
}
