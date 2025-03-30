using Godot;
using System;
using EscapeFromZone.scripts.PlayerScripts;

public partial class MeleeGun : Area2D
{
	public FireTypeImpl FireType;
	public FireTypeImpl PlayerFireType;
	public float AttacksPerSeconds = 1f;
	private float fireRate;
	private float timeUntilFire;

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
		fireRate = 1 / AttacksPerSeconds;
		PlayerFireType = GetTree().Root.GetNode<FireTypeImpl>("main/Player/FireTypeImpl");
	}
	
	public override void _Process(double delta)
	{
		if (timeUntilFire > fireRate && Input.IsActionJustPressed("shoot") && PlayerFireType.fireType == EscapeFromZone.scripts.PlayerScripts.FireType.Melee)
		{
			//Если объект не является живым, то будет просто проигрываться звук анимка удара, без урона
			var list = GetOverlappingAreas();
			foreach (var area in list)
			{
				GD.Print("Бью рукой");
				var parent = area.GetParent();
				if (parent.IsInGroup("Alive"))
				{
					var health = parent.GetNodeOrNull<Health>("Health");
					var host = parent.GetNodeOrNull<HostImpl>("HostImpl");
					GD.Print(parent.GetName() + " is before " + host._host);
					host._host = Host.Enemy;
					GD.Print(parent.GetName() + " is after " + host._host);
					health?.Damage(15);
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
