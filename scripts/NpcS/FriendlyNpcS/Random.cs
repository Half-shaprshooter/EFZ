namespace EscapeFromZone.scripts.FriendlyNpcS;

public partial class Random : NPC_AI
{
	public bool IsStanding;
	private int _patrolPointsVisited = 0;
	public bool CanGoNextPoints;

	public override void _Ready()
	{
		IsStanding = true;
		CanGoNextPoints = false;
		base._Ready();
	}

	public override void _Process(double delta)
	{
		if (_patrolPointsVisited == 5 && !CanGoNextPoints)
		{
			IsStanding = true;
			SetVelocityToZero();
		}
		if (_patrolPointsVisited == 6)
			QueueFree();
		base._Process(delta);
	}

	protected override void HandlePatrol()
	{
		if (!IsStanding)
		{
			base.HandlePatrol();
		}
	}
	
	protected override void AdvancePatrolPoint()
	{
		_patrolPointsVisited++;
		base.AdvancePatrolPoint();
	}
}