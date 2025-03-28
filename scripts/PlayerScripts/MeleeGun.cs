using Godot;
using System;
using EscapeFromZone.scripts.PlayerScripts;

public partial class MeleeGun : Node2D
{
	public FireTypeImpl fireType;
	public FireTypeImpl PlayerFireType;
	float timeUntilFire = 0f;
	private float attackDuration = 0.5f;  // Длительность атаки в секундах
	private float attackRadius = 100f;  // Радиус полукруга
	private float currentTime = 0f;
	private Vector2 startPosition;
	private Sprite2D muzzle;
	
	public int countAttacks = 1;
	
	[Export] PackedScene meleeScene;
	
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

		fireType.fireType = FireType.Melee;
	}
	public override void _Ready()
	{
		PlayerFireType = GetTree().Root.GetNode<FireTypeImpl>("main/Player/FireTypeImpl");
		muzzle = GetTree().Root.GetNode<Sprite2D>("main/Player/muzzle"); 
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("shoot") && PlayerFireType.fireType == FireType.Melee)
		{
			for (int i = 0; i < countAttacks; i++)
			{
				GD.Print("Спавню милишку");
				SpawnMeleeFrame();
			}
			timeUntilFire = 0f;
		}
		else
		{
			timeUntilFire += (float)delta;
		}
	}
	
	public void SpawnMeleeFrame()
	{
		//await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
		
		currentTime += 0.01f;

		RigidBody2D melee = meleeScene.Instantiate<RigidBody2D>();
		melee.GlobalPosition = muzzle.GlobalPosition;
		
		 // Направление пули с учетом разброса
		// float spreadAngle = (float)rnd.RandfRange(-spread, spread) / 100;
		//  Vector2 direction = new Vector2((float)Math.Cos(GlobalRotation), (float)Math.Sin(GlobalRotation));
		//  //direction = direction.Rotated(spreadAngle);
		//
		//  // Устанавливаем позицию, поворот и свойства пули
		//  melee.Rotation = GlobalRotation + spreadAngle;
		//  melee.Set("speed", bulletSpeed);
		//  melee.Set("damage", bulletDamage);
		
		 GetTree().Root.AddChild(melee);
	}
}
