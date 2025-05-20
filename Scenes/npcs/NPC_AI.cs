using EscapeFromZone.scripts.enemyScripts;
using Godot;
using System;
using System.Diagnostics;

public partial class NPC_AI : TalkableNpc
{
    [Export] public Node2D[] PatrolPoints;
    [Export] public CharacterBody2D body2D;
    [Export] public float AttackStopDistance = 400f;
    
    // Параметры для "осматривания"
    [Export] public float LookAroundSpeed = 2.0f;      // Скорость поворота при осматривании
    [Export] public float LookAroundSweepAngle = Mathf.Pi / 3; // Угол обзора в каждую сторону (60 градусов)
    [Export] public float LookAroundPauseTime = 0.75f;   // Время паузы на каждом угле обзора

    public enum BotState { Patrol, Finding, Attack, Panic }

    [Export] private BotState currentState = BotState.Patrol;

    private int _currentPatrolIndex = 0;
    private Vector2[] _patrolTargets;

    private NavigationAgent2D _agent;
    private Line2D _pathLine;

    // Состояние "осматривания"
    private bool _isLookingAround = false;
    private float _lookAroundPhaseTimer = 0f;
    private int _currentLookAroundPhase = 0; 
    // Фазы: 0-неактивно, 1-поворот налево, 2-пауза, 3-поворот направо, 4-пауза, 5-возврат в центр, 6-пауза -> завершение
    private float _initialLookAngle = 0f; // Угол, с которого началось осматривание

    private Vector2? lastSeenPlayerPosition = null;

    private bool isCanBuildRay = false; //отвечает за то, находится ли цель в прямой видимости

    public EnemyGun gun;

    public override async void _Ready()
    {
        _agent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        _pathLine = GetNodeOrNull<Line2D>("PathLine");
        lookRay = GetNode<RayCast2D>("LookRay");
        gun = GetNode<EnemyGun>("Gun");

        if (body2D == null)
        {
            GD.PrintErr("CharacterBody2D (body2D) не назначен для NPC_AI!");
            SetProcess(false); // Отключить обработку, если тело не найдено
            SetPhysicsProcess(false);
            return;
        }

        if (_agent == null || PatrolPoints.Length == 0)
            return;

        _patrolTargets = new Vector2[PatrolPoints.Length];
        for (int i = 0; i < PatrolPoints.Length; i++)
        {
            _patrolTargets[i] = PatrolPoints[i].GlobalPosition;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        _agent.TargetDesiredDistance = 4f;
        SetNextPatrolPoint();
    }

    public override void _Process(double delta)
    {
        relation = body2D.GetNode<HostImpl>("HostImpl");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (target != null)
        {
            isCanBuildRay = BuildRayToTarget(target);
            if (isCanBuildRay && _isLookingAround) // Если нашли игрока во время осматривания
            {
                //GD.Print("Игрок найден во время осматривания. Отмена осматривания.");
                StopLookingAround();
            }
        }
        else // Цели нет
        {
            isCanBuildRay = false;
            if (_isLookingAround) // Если цель пропала во время осматривания
            {
                //GD.Print("Цель исчезла во время осматривания. Отмена осматривания.");
                StopLookingAround();
            }
        }

        StateSystem();

        switch (currentState)
        {
            case BotState.Patrol:
                HandlePatrol();
                break;

            case BotState.Attack:
                ChaseTimer(delta);
                Chase((float)delta);
                break;

            case BotState.Finding:
                break;

            case BotState.Panic:
                break;

            default:
                GD.Print("Я сломался");
                SetVelocityToZero();
                break;
        }

        DrawNavigationPath();

        //GD.Print("видимость ", isCanBuildRay, " есть цель ", target, " поведение ", currentState);
        //GD.Print(timerValue, currentState);

        MoveAndSlide();
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
            RotateTowardsDirectionSmoothly(direction, (float)GetPhysicsProcessDeltaTime(), RotationSpeed);

            // Движение
            SetVelocityWalk(direction);
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

    private void Chase(float delta)
    {
        if (target == null)
        {
            if (_isLookingAround) StopLookingAround();
            SetVelocityToZero();
            return;
        }

        if (isCanBuildRay) // Если есть прямая видимость
        {
            if (_isLookingAround) StopLookingAround(); // Прекратить осматриваться, если игрок появился

            if (isInSeeZone && isCanBuildRay)
            {
                gun.makeNear(true);
            }
            else
            {
                gun.makeNear(false);
            }

            lastSeenPlayerPosition = target.GlobalPosition;
            _agent.TargetPosition = target.GlobalPosition;

            float distanceToPlayer = GlobalPosition.DistanceTo(target.GlobalPosition);

            if (distanceToPlayer > AttackStopDistance)
            {
                Vector2 nextNavPathPosition = _agent.GetNextPathPosition();
                Vector2 directionToNextNavPos = (nextNavPathPosition - GlobalPosition).Normalized();

                int speedLevel;
                if (distanceToPlayer < 500 || timerValue > 2.5) { speedLevel = 1; }
                else if (distanceToPlayer < 800) { speedLevel = 2; }
                else { speedLevel = 3; }
                MoveToTarget(directionToNextNavPos, delta, speedLevel);
            }
            else
            {
                var directionToTarget = (target.GlobalPosition - GlobalPosition).Normalized();
                RotateTowardsDirectionSmoothly(directionToTarget, delta, BattleRotationSpeed);
                SetVelocityToZero();
            }
        }
        else if (lastSeenPlayerPosition.HasValue) // Прямой видимости нет, НО есть последняя известная позиция
        {
            gun.makeNear(false);
            
            _agent.TargetPosition = lastSeenPlayerPosition.Value;
            float distanceToLastSeenPos = GlobalPosition.DistanceTo(lastSeenPlayerPosition.Value);

            // Двигаемся к LSPP, если еще не там И не в процессе осматривания
            if (distanceToLastSeenPos > _agent.TargetDesiredDistance + 5.0f && !_isLookingAround)
            {
                Vector2 nextNavPathPosition = _agent.GetNextPathPosition();
                Vector2 directionToNextNavPos = (nextNavPathPosition - GlobalPosition).Normalized();
                MoveToTarget(directionToNextNavPos, delta, 1); // Level 1 = WalkSpeed
            }
            else // Достигли LSPP (или достаточно близко) ИЛИ уже осматриваемся
            {
                SetVelocityToZero(); // Останавливаемся
                if (!_isLookingAround) // Если еще не начали осматриваться
                {
                    StartLookingAround();
                }
                HandleLookingAround(delta); // Выполняем логику осматривания
            }
        }
        else // Нет прямой видимости И НЕТ последней известной позиции
        {
            if (_isLookingAround) StopLookingAround();
            SetVelocityToZero();
            // StateSystem или ChaseTimer должны обработать и перевести в Patrol
        }
    }

    private void StartLookingAround()
    {
        if (_isLookingAround) return;
        GD.Print("Начинаю осматриваться...");
        _isLookingAround = true;
        _currentLookAroundPhase = 1; // Начать с первой фазы
        _lookAroundPhaseTimer = 0f;
        _initialLookAngle = body2D.Rotation;
    }

    private void StopLookingAround()
    {
        if (!_isLookingAround) return;
        GD.Print("Заканчиваю осматриваться.");
        _isLookingAround = false;
        _currentLookAroundPhase = 0;
        _lookAroundPhaseTimer = 0f;
        // Можно добавить поворот в сторону _initialLookAngle, если нужно вернуться к исходному направлению
        // RotateTowardsAngleSmoothly(_initialLookAngle, GetPhysicsProcessDeltaTime(), LookAroundSpeed);
    }

    private void HandleLookingAround(double delta)
    {
        if (!_isLookingAround || target == null) // Если осматривание было прервано или цель исчезла
        {
            StopLookingAround();
            return;
        }

        _lookAroundPhaseTimer += (float)delta;
        float targetAngleThisPhase = body2D.Rotation; // Текущий угол по умолчанию

        switch (_currentLookAroundPhase)
        {
            case 1: // Поворот налево
                targetAngleThisPhase = _initialLookAngle - LookAroundSweepAngle;
                RotateTowardsAngleSmoothly(targetAngleThisPhase, (float)delta, LookAroundSpeed);
                if (Mathf.Abs(Mathf.AngleDifference(body2D.Rotation, targetAngleThisPhase)) < Mathf.DegToRad(5)) // Достигли угла
                {
                    _currentLookAroundPhase = 2; _lookAroundPhaseTimer = 0f;
                }
                break;
            case 2: // Пауза (смотрим налево)
                if (_lookAroundPhaseTimer >= LookAroundPauseTime)
                {
                    _currentLookAroundPhase = 3; _lookAroundPhaseTimer = 0f;
                }
                break;
            case 3: // Поворот направо
                targetAngleThisPhase = _initialLookAngle + LookAroundSweepAngle;
                RotateTowardsAngleSmoothly(targetAngleThisPhase, (float)delta, LookAroundSpeed);
                 if (Mathf.Abs(Mathf.AngleDifference(body2D.Rotation, targetAngleThisPhase)) < Mathf.DegToRad(5))
                {
                    _currentLookAroundPhase = 4; _lookAroundPhaseTimer = 0f;
                }
                break;
            case 4: // Пауза (смотрим направо)
                if (_lookAroundPhaseTimer >= LookAroundPauseTime)
                {
                    _currentLookAroundPhase = 5; _lookAroundPhaseTimer = 0f;
                }
                break;
            case 5: // Поворот в центр (исходное направление)
                targetAngleThisPhase = _initialLookAngle;
                RotateTowardsAngleSmoothly(targetAngleThisPhase, (float)delta, LookAroundSpeed);
                if (Mathf.Abs(Mathf.AngleDifference(body2D.Rotation, targetAngleThisPhase)) < Mathf.DegToRad(5))
                {
                    _currentLookAroundPhase = 6; _lookAroundPhaseTimer = 0f;
                }
                break;
            case 6: // Пауза (смотрим в центр)
                 if (_lookAroundPhaseTimer >= LookAroundPauseTime)
                {
                    GD.Print("Завершил последовательность осматривания.");
                    StopLookingAround(); // Завершаем осматривание
                    // ChaseTimer продолжит отсчет, если игрок так и не найден, и вернет в Patrol
                }
                break;
        }
    }

    private void MoveToTarget(Vector2 targetPosition, float delta, int levelSpeed)
    {
        if (_agent == null)
            return;

        var nextPos = _agent.GetNextPathPosition();
        var direction = nextPos - GlobalPosition;

        RotateTowardsDirectionSmoothly(direction, delta, BattleRotationSpeed);
        switch (levelSpeed)
        {
            case 1:
                SetVelocityWalk(direction);
                break;
            case 2:
                SetVelocityFast(direction);
                break;
            case 3:
                SetVelocityRun(direction);
                break;
        }
    }

    private void SetVelocityToZero()
    {
        body2D.Velocity = Vector2.Zero;
    }

    private RayCast2D lookRay;
    private Node2D target;
    //построение луча до цели, что понять, есть ли прямая видимость
    private bool BuildRayToTarget(Node2D target)
	{
		if (target == null) return false;
		//lookRay.Rotation = body2D.Rotation;
		
		// Устанавливаем начало луча в (0, 0) — относительно RayCast2D, он сам будет сдвинут в нужную точку
		lookRay.Position = Vector2.Zero;
		lookRay.GlobalPosition = body2D.GlobalPosition;

		// Переводим глобальные координаты цели в локальные относительно RayCast2D
		Vector2 localTarget = lookRay.ToLocal(target.GlobalPosition);
		lookRay.TargetPosition = localTarget;
        lookRay.ForceRaycastUpdate();

		// Если луч столкнулся с объектом (целевым)
		if (lookRay.IsColliding() && lookRay.GetCollider() == target)
		{
			// Показываем текст, если луч построен и объект видим
			return true;
		}
		return false;
	}

    private void StateSystem()
    {
        if (target != null && relation._host == Host.Enemy)
        {
            currentState = BotState.Attack; // Внутри Attack будет логика преследования, осматривания и т.д.
        }
        else
        {
            if (_isLookingAround) // Если цель пропала (target стал null), а мы осматривались
            {
                StopLookingAround();
            }
            currentState = BotState.Patrol;
            // lastSeenPlayerPosition = null; // Сбрасывается в ChaseTimer или когда target становится null
        }
    }

    double timerValue = 0;
    private void ChaseTimer(double delta)
    {
        // Таймер тикает, если есть цель, но мы её не видим (ни лучом, ни в зонах)
        bool canTrackPlayer = isCanBuildRay || isInSeeZone || isKnowPosBattle;

        if (target != null && !canTrackPlayer)
        {
            timerValue += 1 * delta;
            if (timerValue >= 10) // Таймаут 10 секунд
            {
                GD.Print("ChaseTimer: Время вышло, переход в патруль.");
                if(_isLookingAround) // Если таймер истек во время осматривания
                {
                    StopLookingAround();
                }
                currentState = BotState.Patrol;
                target = null;
                lastSeenPlayerPosition = null; // Важно сбросить
                timerValue = 0;
                // SetNextPatrolPoint(); // Вызовется при активации HandlePatrol
            }
        }
        else
        {
            timerValue = 0; // Сбросить таймер, если видим игрока, он в зоне, или цели нет
        }
    }

    private bool isInSeeZone = false;
    //Зона видимости бота(вход)
    private void SeeZoneEnter(Node2D body)
    {
        if (body is PlayerControl)
        {
            isInSeeZone = true;
            target = body;
        }
    }
    //Зона видимости бота(выход)
    private void SeeZoneExited(Node2D body)
    {
        if (body is PlayerControl)
        {
            isInSeeZone = false;
        }
    }

    //Зона в которой в бою бот всегда знает где игрок, если есть прямая видимость
    private bool isKnowPosBattle = false;
    private void KnowPosInChaseEntered(Node2D body)
    {
        if (body is PlayerControl)
        {
            isKnowPosBattle = true;
        }
    }

    private void KnowPosInChaseExited(Node2D body)
    {
        if (body is PlayerControl)
        {
            isKnowPosBattle = false;
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
}
