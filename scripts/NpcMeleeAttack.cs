// NpcMeleeAttack.cs
using Godot;
using System;
using System.Collections.Generic; // Для List и HashSet
								  // using EscapeFromZone.scripts.PlayerScripts; // Это пространство имен больше не нужно напрямую, если FireTypeImpl не используется для NPC

public partial class NpcMeleeAttack : Area2D
{
	private AudioStreamPlayer2D _effectsPlayer; // Переименовал для ясности

	// Звуки можно сделать экспортируемыми, чтобы настраивать для разных NPC
	[Export] private AudioStream _hitAliveSound; // = (AudioStream)GD.Load("res://sounds/effects/KnifeAttack.mp3");
	[Export] private AudioStream[] _hitDestructibleSounds; // = new AudioStream[] {(AudioStream)GD.Load("res://sounds/effects/BoxSmash.mp3"), (AudioStream)GD.Load("res://sounds/effects/DoorSmash.mp3")};

	// public FireTypeImpl NpcFireType; // Если у NPC будет система типов оружия, аналогичная игроку

	[Export] public float AttacksPerSecond = 1f;
	private float _fireRate;
	private float _timeSinceLastAttack = 0f; // Таймер для отслеживания кулдауна

	// Ссылка на CharacterBody2D, который является владельцем этого оружия (сам NPC)
	// Должна быть установлена извне (например, в NPC_AI._Ready())
	public CharacterBody2D AttackerBody { get; set; }

	private PlayerControl Player;

	// Конструктор - можно оставить, если FireTypeImpl нужен для чего-то общего,
	// но его привязка к PlayerScripts.FireType.Melee здесь уже не так актуальна.
	public NpcMeleeAttack()
	{
		// Если FireTypeImpl используется для общих характеристик оружия (не только для типа игрока),
		// то его создание здесь может быть оправдано.
		// var fireType = this.GetNodeOrNull<FireTypeImpl>("FireTypeImpl");
		// if (fireType == null)
		// {
		//     fireType = new FireTypeImpl();
		//     this.AddChild(fireType);
		//     fireType.Name = "FireTypeImpl";
		// }
		// fireType.fireType = EscapeFromZone.scripts.PlayerScripts.FireType.Melee; // Возможно, это нужно убрать или изменить
	}

	public override void _Ready()
	{
		_effectsPlayer = GetNodeOrNull<AudioStreamPlayer2D>("EffectsPlayerKnife"); // Убедитесь, что имя узла верное
		if (_effectsPlayer == null)
		{
			GD.PrintErr($"NpcMeleeAttack ({Name}): Node 'EffectsPlayerKnife' not found!");
		}

		if (AttacksPerSecond <= 0)
		{
			GD.PrintErr($"NpcMeleeAttack ({Name}): AttacksPerSecond must be greater than 0. Defaulting to 1.");
			AttacksPerSecond = 1f;
		}
		_fireRate = 1 / AttacksPerSecond;
		_timeSinceLastAttack = _fireRate; // Чтобы можно было атаковать сразу
	}

	public override void _Process(double delta)
	{
		// Обновляем таймер кулдауна
		if (_timeSinceLastAttack < _fireRate)
		{
			_timeSinceLastAttack += (float)delta;
		}
	}

	// Метод для инициации атаки со стороны NPC
	public bool CanAttack()
	{
		return _timeSinceLastAttack >= _fireRate;
	}

	public void PerformAttack()
	{
		if (!CanAttack())
		{
			// GD.Print($"NpcMeleeAttack ({AttackerBody?.Name ?? "N/A"}): Attack on cooldown.");
			return;
		}

		if (AttackerBody == null)
		{
			GD.PrintErr($"NpcMeleeAttack ({Name}): PerformAttack called, but AttackerBody is null! Cannot determine damage source.");
			return;
		}

		GD.Print($"NpcMeleeAttack: NPC '{AttackerBody.Name}' is attacking.");

		var overlappingAreas = GetOverlappingAreas();
		var damagedEntitiesThisSwing = new HashSet<Node>(); // Предотвращает многократный урон одной цели за один удар

		bool attackHitSomething = false;
		GD.Print(overlappingAreas);
		foreach (var area in overlappingAreas)
		{
			var parentOfArea = area.GetParent();
			if (parentOfArea == null || parentOfArea == AttackerBody) // Не атаковать себя
			{
				continue;
			}

			if (damagedEntitiesThisSwing.Contains(parentOfArea))
			{
				continue; // Уже атаковали эту цель в этом "взмахе"
			}

			bool didDamageThisTarget = false;
			if (parentOfArea.IsInGroup("Player"))
			{
				PlaySound(_hitAliveSound);

				GD.Print($"NpcMeleeAttack: NPC '{AttackerBody.Name}' hit 'Alive' entity: {parentOfArea.Name}.");
				Player.TakeDmg(30);
				didDamageThisTarget = true;
				attackHitSomething = true;
			}
			else if (parentOfArea.IsInGroup("Distructable"))
			{
				if (_hitDestructibleSounds != null && _hitDestructibleSounds.Length > 0)
				{
					Random random = new Random();
					PlaySound(_hitDestructibleSounds[random.Next(0, _hitDestructibleSounds.Length)]);
				}
				var health = parentOfArea.GetNodeOrNull<Health>("Health");
				// GD.Print($"NpcMeleeAttack: NPC '{AttackerBody.Name}' hit 'Distructable' entity: {parentOfArea.Name}");
				health?.Damage(15, AttackerBody); // Передаем CharacterBody2D атакующего (NPC)
				didDamageThisTarget = true;
				attackHitSomething = true;
			}

			if (didDamageThisTarget)
			{
				damagedEntitiesThisSwing.Add(parentOfArea);
			}
		}

		if (attackHitSomething)
		{
			//GD.Print($"NpcMeleeAttack: NPC '{AttackerBody.Name}' attack hit targets.");
		}
		else
		{
			// GD.Print($"NpcMeleeAttack: NPC '{AttackerBody.Name}' attack missed (no valid targets in range).");
			// Можно проигрывать звук промаха, если нужно
		}
		_timeSinceLastAttack = 0f; // Сбросить таймер кулдауна
	}

	private void PlaySound(AudioStream sound)
	{
		if (_effectsPlayer != null && sound != null)
		{
			_effectsPlayer.Stream = sound;
			_effectsPlayer.Play();
		}
	}

	private void AttackZoneEnter(Node2D body)
	{
		if (body is PlayerControl)
		{
			GD.Print("Нашёл");
			Player = body as PlayerControl;
			GD.Print(Player);
		}
	}

	private void AttackZoneExit(Node2D body)
	{
		Player = null;
	}
}

