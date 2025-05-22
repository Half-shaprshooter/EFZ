using EscapeFromZone.scripts.enemyScripts;
using Godot;
using System;
using System.Diagnostics;

public partial class NPC_AI : TalkableNpc
{
	[Export] public Node2D[] PatrolPoints;
	[Export] public CharacterBody2D body2D;
	[Export] public float AttackStopDistance = 400f;
	[Export] public float MeleeAttackRange = 70f;
	[Export] public bool CanUseRangedWeapon = true;
	[Export] public bool CanUseMeleeWeapon = true; 

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
	private NpcMeleeAttack _npcMeleeWeapon;

	private Health _npcHealth;

	public override async void _Ready()
	{
		_agent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
		_pathLine = GetNodeOrNull<Line2D>("PathLine");
		lookRay = GetNode<RayCast2D>("LookRay");
		gun = GetNode<EnemyGun>("Gun");
		_npcMeleeWeapon = GetNodeOrNull<NpcMeleeAttack>("NpcMeleeAttack");

		if (body2D == null)
		{
			GD.PrintErr("CharacterBody2D (body2D) не назначен для NPC_AI!");
			SetProcess(false); // Отключить обработку, если тело не найдено
			SetPhysicsProcess(false);
			return;
		}

		_npcHealth = GetNodeOrNull<Health>("Health");
		if (_npcHealth != null)
		{
			_npcHealth.Damaged += OnDamaged;
		}

		_npcMeleeWeapon.AttackerBody = this.body2D; 

		_patrolTargets = new Vector2[PatrolPoints.Length];
		for (int i = 0; i < PatrolPoints.Length; i++)
		{
			_patrolTargets[i] = PatrolPoints[i].GlobalPosition;
		}

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		_agent.TargetDesiredDistance = 4f;
		SetNextPatrolPoint();
		relation = GetNodeOrNull<HostImpl>("HostImpl");
		relation._host = relationToPlayer;
	}

	public override void _Process(double delta)
	{
		relation = GetNodeOrNull<HostImpl>("HostImpl");
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
				currentState = BotState.Patrol;
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
			if (CanUseRangedWeapon && gun != null) gun.makeNear(false); // Отключаем прицеливание, если есть оружие
			return;
		}

		float distanceToTarget = GlobalPosition.DistanceTo(target.GlobalPosition);

		// Приоритет ближней атаке, если возможно и разрешено
		if (CanUseMeleeWeapon && _npcMeleeWeapon != null && (distanceToTarget <= MeleeAttackRange || distanceToTarget - MeleeAttackRange < 30))
		{
			SetVelocityToZero();
			var directionToTarget = (target.GlobalPosition - GlobalPosition).Normalized();
			RotateTowardsDirectionSmoothly(directionToTarget, delta, BattleRotationSpeed);

			if (_npcMeleeWeapon.CanAttack())
			{
				//GD.Print($"NPC_AI ({Name}): Performing MELEE attack on {target.Name}!");
				_npcMeleeWeapon.PerformAttack();
			}
			// Если атакуем вблизи, дальнее оружие не должно целиться/стрелять
			if (CanUseRangedWeapon && gun != null) gun.makeNear(false);
		}
		// Логика дальней атаки или преследования, если ближняя атака невозможна/не разрешена или цель дальше
		else
		{
			if (isCanBuildRay) // Если есть прямая видимость
			{
				if (_isLookingAround) StopLookingAround();
				
				lastSeenPlayerPosition = target.GlobalPosition;
				if (_agent != null && _agent.IsInsideTree()) _agent.TargetPosition = target.GlobalPosition;

				// Если можем использовать дальнобойное оружие и оно есть
				if (CanUseRangedWeapon && gun != null)
				{
					gun.makeNear(true); // Целимся из дальнего оружия

					if (distanceToTarget > AttackStopDistance) // Далеко для стрельбы, но видим - преследуем
					{
						// Логика преследования (MoveToTarget)
						if (_agent != null && _agent.IsInsideTree())
						{
							Vector2 nextNavPathPosition = _agent.GetNextPathPosition();
							Vector2 directionToNextNavPos = (nextNavPathPosition - GlobalPosition).Normalized();
							int speedLevel = (distanceToTarget < 500 || timerValue > 2.5) ? 1 : (distanceToTarget < 800 ? 2 : 3);
							MoveToTarget(directionToNextNavPos, delta, speedLevel);
						} else { SetVelocityToZero(); }
					}
					else // Достаточно близко для дальней атаки (и не в зоне ближней, или ближняя запрещена)
					{
						var directionToTarget = (target.GlobalPosition - GlobalPosition).Normalized();
						RotateTowardsDirectionSmoothly(directionToTarget, delta, BattleRotationSpeed);
						SetVelocityToZero();
						// Здесь должна быть логика стрельбы из 'gun'
						// Например, если у EnemyGun есть метод TryShoot(Node2D target)
						// gun.TryShoot(target); 
						//GD.Print($"NPC_AI ({Name}): In RANGED attack range. Would shoot at {target.Name} if gun.TryShoot() implemented.");
					}
				}
				// Если не можем использовать дальнобойное, но видим цель - просто преследуем (если разрешена ближняя и цель далеко)
				// или стоим, если преследовать нечем/незачем.
				else if (CanUseMeleeWeapon) // Если разрешена только ближняя, а цель далеко - преследуем
				{
					 if (distanceToTarget > MeleeAttackRange + 5.0f) // Небольшой буфер, чтобы не дергался
					 {
						if (_agent != null && _agent.IsInsideTree())
						{
							Vector2 nextNavPathPosition = _agent.GetNextPathPosition();
							Vector2 directionToNextNavPos = (nextNavPathPosition - GlobalPosition).Normalized();
							 // Можно использовать другую скорость, если это просто преследование для ближнего боя
							MoveToTarget(directionToNextNavPos, delta, 1); // Двигаемся медленнее
						} else { SetVelocityToZero(); }
					 }
					 else // Слишком близко для "только преследования", но не в MeleeAttackRange (или ближняя не сработала выше) - стоим и смотрим
					 {
						var directionToTarget = (target.GlobalPosition - GlobalPosition).Normalized();
						RotateTowardsDirectionSmoothly(directionToTarget, delta, BattleRotationSpeed);
						SetVelocityToZero();
					 }
				}
				else // Не может использовать ни дальнее, ни ближнее - просто стоит и смотрит, если видит
				{
					var directionToTarget = (target.GlobalPosition - GlobalPosition).Normalized();
					RotateTowardsDirectionSmoothly(directionToTarget, delta, BattleRotationSpeed);
					SetVelocityToZero();
					GD.Print($"NPC_AI ({Name}): Sees target {target.Name} but cannot use any weapon.");
				}
			}
			else if (lastSeenPlayerPosition.HasValue) // Прямой видимости нет, НО есть последняя известная позиция
			{
				if (CanUseRangedWeapon && gun != null) gun.makeNear(false);
				
				if (_agent != null && _agent.IsInsideTree()) _agent.TargetPosition = lastSeenPlayerPosition.Value;
				float distanceToLastSeenPos = GlobalPosition.DistanceTo(lastSeenPlayerPosition.Value);

				if (distanceToLastSeenPos > (_agent?.TargetDesiredDistance ?? 4f) + 5.0f && !_isLookingAround)
				{
					 if (_agent != null && _agent.IsInsideTree())
					 {
						Vector2 nextNavPathPosition = _agent.GetNextPathPosition();
						Vector2 directionToNextNavPos = (nextNavPathPosition - GlobalPosition).Normalized();
						MoveToTarget(directionToNextNavPos, delta, 1);
					 } else { SetVelocityToZero(); }
				}
				else
				{
					SetVelocityToZero();
					if (!_isLookingAround) StartLookingAround();
					HandleLookingAround(delta);
				}
			}
			else // Совсем потеряли цель
			{
				if (_isLookingAround) StopLookingAround();
				SetVelocityToZero();
				if (CanUseRangedWeapon && gun != null) gun.makeNear(false);
			}
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
		if (relation == null)
		{
			GD.Print("Отношения сломаны");
			return;
		}
		
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
				if (_isLookingAround) // Если таймер истек во время осматривания
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

	private void OnDamaged(float damageAmount, Node attackerNode) // attackerNode это Node из сигнала
	{
		GD.Print($"NPC_AI ({Name}): I've been damaged by '{(attackerNode != null ? attackerNode.Name : "Unknown")}' for {damageAmount} HP!");

		if (attackerNode == null)
		{
			GD.Print($"NPC_AI ({Name}): Attacker is null. Current state: {currentState}");
			return;
		}

		Node2D attackerAsNode2D = attackerNode as Node2D;
		if (attackerAsNode2D == null)
		{
			GD.PrintErr($"NPC_AI ({Name}): Attacker '{attackerNode.Name}' is not a Node2D. Cannot process as target.");
			return;
		}
		
		this.target = attackerAsNode2D; // Устанавливаем Node2D как цель

		if (this.relation == null)
		{
			// Попытка получить 'relation' еще раз, если он null
			this.relation = body2D?.GetNodeOrNull<HostImpl>("HostImpl");
			if (this.relation == null)
			{
				GD.PrintErr($"NPC_AI ({Name}): Relation component is null. Cannot change hostility towards {this.target.Name}.");
				return; 
			}
		}

		this.relation._host = Host.Enemy;
		GD.Print($"NPC_AI ({Name}): Relation with {this.target.Name} set to Host.Enemy.");
		
		currentState = BotState.Attack;
		lastSeenPlayerPosition = this.target.GlobalPosition; 

		if (_isLookingAround) StopLookingAround();
		timerValue = 0; // Сброс таймера погони
		// Можно также сбросить кулдаун атаки, чтобы NPC мог сразу контратаковать, если это _npcMeleeWeapon
		// _npcMeleeWeapon?.ResetCooldown(); // (потребует добавить такой метод в NpcMeleeAttack)

		GD.Print($"NPC_AI ({Name}): Target set to {this.target.Name}. State: Attack. LSPP: {lastSeenPlayerPosition}.");
	}

	// Важно отписаться от сигнала при уничтожении узла
	public override void _ExitTree()
	{
		if (_npcHealth != null)
		{
			_npcHealth.Damaged -= OnDamaged;
		}
		base._ExitTree(); // Вызов базового метода, если он есть и что-то делает
	}
}
