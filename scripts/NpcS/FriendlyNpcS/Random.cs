namespace EscapeFromZone.scripts.FriendlyNpcS;

public partial class Random : NPC_AI
{
	public bool IsStanding;
	private int _patrolPointsVisited = 0;
	[Export] public Node2D[] NextPatrolPoints;

	public override void _Ready()
	{
		IsStanding = true;
		base._Ready();
	}

	public override void _Process(double delta)
	{
		if (_patrolPointsVisited == PatrolPoints.Length)
		{
			_patrolPointsVisited = 0;
			IsStanding = false;
			SetVelocityToZero();
		}
		base._Process(delta);
	}

	protected override void HandlePatrol()
	{
		if (!IsStanding)
		{
			base.HandlePatrol();
		}
	}

	public void SetNextPoints()
	{
		PatrolPoints = NextPatrolPoints;
        
		// 2. Обновляем массив позиций
		_patrolTargets = new Vector2[PatrolPoints.Length];
		for (int i = 0; i < PatrolPoints.Length; i++)
		{
			_patrolTargets[i] = PatrolPoints[i].GlobalPosition;
		}
        
		// 3. Сбрасываем индекс и счетчик
		_currentPatrolIndex = 0;
		_patrolPointsVisited = 0;
        
		// 4. Запускаем движение к первой точке
		SetNextPatrolPoint();
	}
}