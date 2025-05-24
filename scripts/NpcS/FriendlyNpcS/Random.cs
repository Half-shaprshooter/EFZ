namespace EscapeFromZone.scripts.FriendlyNpcS;

public partial class Random : NPC_AI
{
	public bool IsStanding;

	public override void _Ready()
	{
		IsStanding = true;
		base._Ready();
	}

	protected override void HandlePatrol()
	{
		if (!IsStanding)
		{
			base.HandlePatrol();
		}
	}
}