using Godot;
using System;
using EscapeFromZone.scripts.PlayerScripts;

public partial class MeleeGun : Area2D
{
	private AudioStreamPlayer2D effects;
	
	private AudioStream FIRST_SOUND = (AudioStream)GD.Load("res://sounds/effects/BoxSmash.mp3");
	private AudioStream SECOND_SOUND = (AudioStream)GD.Load("res://sounds/effects/DoorSmash.mp3");
	private AudioStream KNIFE_SOUND = (AudioStream)GD.Load("res://sounds/effects/KnifeAttack.mp3");

	private List<AudioStream> _list = new List<AudioStream>();
	
	public FireTypeImpl FireType;
	public FireTypeImpl PlayerFireType;
	public float AttacksPerSeconds = 1f;
	private float fireRate;
	private float timeUntilFire;
	[Export] private CharacterBody2D whoAttacks = null;

	public MeleeGun()
	{
		var fireType = this.GetNodeOrNull<FireTypeImpl>("FireTypeImpl");

		if (fireType == null)
		{
			GD.Print("Добавил");
			fireType = new FireTypeImpl();
			this.AddChild(fireType);
			fireType.Name = "FireTypeImpl";
		}

		fireType.fireType = EscapeFromZone.scripts.PlayerScripts.FireType.Melee;
	}

	public override void _Ready()
	{
		_list.Add(SECOND_SOUND);
		_list.Add(FIRST_SOUND);
		effects = GetNode<AudioStreamPlayer2D>("EffectsPlayerKnife");
		fireRate = 1 / AttacksPerSeconds;
		PlayerFireType = GetTree().Root.GetNode<FireTypeImpl>("main/Player/FireTypeImpl");
	}
	
	public override void _Process(double delta)
	{
		if (!PlayerData.CanFire)
		{
			return;
		}
		if (timeUntilFire > fireRate && Input.IsActionJustPressed("shoot") && PlayerFireType.fireType == EscapeFromZone.scripts.PlayerScripts.FireType.Melee)
		{
			//Если объект не является живым, то будет просто проигрываться звук анимка удара, без урона
			var list = GetOverlappingAreas();
			foreach (var area in list)
			{
				if (area.Name == "HitBox")
				{
					var parent = area.GetParent();
					if (parent.IsInGroup("Alive"))
					{
						GD.Print("Melee Gun: Попытка нанести урон Alive");
						effects.Stream = KNIFE_SOUND;
						effects.Play();
						var health = parent.GetNodeOrNull<Health>("Health");
						var host = parent.GetNodeOrNull<HostImpl>("HostImpl");
						GD.Print(parent);
						GD.Print(parent.GetName() + " is before " + host._host);
						host._host = Host.Enemy;
						GD.Print(parent.GetName() + " is after " + host._host);
						//TODO: Если милишки будут разные, сюда нужно будет передавать динамично урон
						health?.Damage(15, whoAttacks);
					}
					if (parent.IsInGroup("Distructable"))
					{
						Random random = new Random();
						effects.Stream = _list[random.Next(0, 2)];
						effects.Play();
						var health = parent.GetNodeOrNull<Health>("Health");
						GD.Print(parent);
						GD.Print("Damage");
						health?.Damage(15);
					}
				}
			}
			timeUntilFire = 0f;
		}
		else
		{
			timeUntilFire += (float)delta;
		}
	}
}
