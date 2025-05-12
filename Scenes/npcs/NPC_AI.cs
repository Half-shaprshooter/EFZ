using Godot;
using System;

public partial class NPC_AI : TalkableNpc
{
    [Export] public Node2D[] PatrolPoints;
    [Export] public CharacterBody2D body;
    [Export] public float AttackStopDistance = 400f;
    private int _currentPatrolIndex = 0;
    private Vector2[] _patrolTargets;

    private NavigationAgent2D _agent;
    private Line2D _pathLine;

    private Vector2? lastSeenPlayerPosition = null;

    public override async void _Ready()
    {
        _agent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        _pathLine = GetNodeOrNull<Line2D>("PathLine");
        lookRay = GetNode<RayCast2D>("LookRay");

        if (_agent == null || PatrolPoints.Length == 0)
            return;

        _patrolTargets = new Vector2[PatrolPoints.Length];
        for (int i = 0; i < PatrolPoints.Length; i++)
        {
            _patrolTargets[i] = PatrolPoints[i].GlobalPosition;
        }

        await ToSignal(GetTree(), "process_frame");

        _agent.TargetDesiredDistance = 4f;
        SetNextPatrolPoint();
    }

    public override void _Process(double delta)
    {
        relation = body.GetNode<HostImpl>("HostImpl");

        if (seePlayer)
        {
            BuildRayToTarget(target);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (target != null && seePlayer)
            Fight((float)delta);
        else
            HandlePatrol();

        DrawNavigationPath();
    }

    //логика при патруле
    private void HandlePatrol()
    {
        if (_agent == null || PatrolPoints.Length == 0)
            return;

        if (_agent.IsNavigationFinished())
        {
            AdvancePatrolPoint();
            SetNextPatrolPoint();
        }
        else
        {
            var nextPos = _agent.GetNextPathPosition();
            var direction = nextPos - GlobalPosition;

            // Плавный поворот
            RotateTowardsDirectionSmoothly(direction, body, (float)GetPhysicsProcessDeltaTime(), RotationSpeed);

            // Движение
            MoveInDirectionWalk(direction);
        }
    }

    //рисует направление пути бота
    private void DrawNavigationPath()
    {
        if (_pathLine == null || _agent == null)
            return;

        var path = _agent.GetCurrentNavigationPath();
        _pathLine.ClearPoints();

        foreach (var point in path)
        {
            _pathLine.AddPoint(ToLocal(point)); // локальные координаты от NPC
        }
    }

    private void AdvancePatrolPoint()
    {
        _currentPatrolIndex = (_currentPatrolIndex + 1) % PatrolPoints.Length;
    }

    //выбирает следующую точку патруля
    private void SetNextPatrolPoint()
    {
        if (_agent != null && PatrolPoints.Length > 0)
        {
            var target = _patrolTargets[_currentPatrolIndex];
            _agent.TargetPosition = target;
        }
    }

    private void Fight(float delta)
    {
        if (target == null)
            return;

        bool hasLineOfSight = BuildRayToTarget(target);

        if (hasLineOfSight)
        {
            lastSeenPlayerPosition = target.GlobalPosition;

            float distanceToPlayer = GlobalPosition.DistanceTo(target.GlobalPosition);

            if (distanceToPlayer > AttackStopDistance)
            {
                _agent.TargetPosition = target.GlobalPosition;

                // Только двигаемся, если точка дальше AttackStopDistance
                MoveToTarget(target.GlobalPosition, delta);
            }
            else
            {
                // Останавливаем движение И НЕ ЗАДАЁМ TargetPosition
                Velocity = Vector2.Zero;
                MoveAndSlide();

                RotateTowardsDirectionSmoothly(target.GlobalPosition - GlobalPosition, body, delta, BattleRotationSpeed);

                GD.Print("Атакую игрока!");
                // TODO: Атака
            }
        }
        else if (lastSeenPlayerPosition != null)
        {
            float distance = GlobalPosition.DistanceTo(lastSeenPlayerPosition.Value);

            if (distance > 10f)
            {
                _agent.TargetPosition = lastSeenPlayerPosition.Value;
                MoveToTarget(lastSeenPlayerPosition.Value, delta);
            }
            else
            {
                lastSeenPlayerPosition = null;
                SetNextPatrolPoint();
            }
        }
    }

    private bool seePlayer = false;
    //Зона видимости бота(вход)
    private void SeeZoneEnter(Node2D body)
    {
        if (body is PlayerControl)
        {
            seePlayer = true;
            target = body;
        }
    }

    //Зона видимости бота(выход)
    private void SeeZoneExited(Node2D body)
    {
        if (body is PlayerControl)
        {
            seePlayer = false;
            target = null;
        }
    }

    private void MoveToTarget(Vector2 targetPosition, float delta)
    {
        if (_agent == null)
            return;

        var nextPos = _agent.GetNextPathPosition();
        var direction = nextPos - GlobalPosition;

        RotateTowardsDirectionSmoothly(direction, body, delta, RotationSpeed);
        MoveInDirectionRun(direction);
    }

    private RayCast2D lookRay;
    private Node2D target;
    //построение луча до цели, что понять, есть ли прямая видимость
    private bool BuildRayToTarget(Node2D target)
	{
		if (target == null) return false;
		lookRay.Rotation = body.Rotation;
		
		// Устанавливаем начало луча в (0, 0) — относительно RayCast2D, он сам будет сдвинут в нужную точку
		lookRay.Position = Vector2.Zero;
		lookRay.GlobalPosition = body.GlobalPosition;

		// Переводим глобальные координаты цели в локальные относительно RayCast2D
		Vector2 localTarget = lookRay.ToLocal(target.GlobalPosition);
		lookRay.TargetPosition = localTarget;

		// Если луч столкнулся с объектом (целевым)
		if (lookRay.IsColliding() && lookRay.GetCollider() == target)
		{
			// Показываем текст, если луч построен и объект видим
			return true;
		}
		return false;
	}
}
