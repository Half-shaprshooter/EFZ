using Godot;
using System;

public partial class Gun : Node2D
{
	[Export] public int spread = 10; // Разброс
	[Export] public bool isAuto = true; // Автоматический режим
	[Export] public float bulletsPerSecond = 5f; // Скорострельность
	[Export] public float bulletSpeed = 10000f; // Скорость пули
	[Export] public float bulletDamage = 30f; // Урон пули
	[Export] public int bulletsPerShoot = 1; // Количество пуль за выстрел

	[Export] PackedScene bulletScene;

	float fireRate;
	float timeUntilFire = 0f;

	private Timer muzzleTimer;
	private Sprite2D muzzle;

	public override void _Ready()
	{
		fireRate = 1 / bulletsPerSecond;

		muzzleTimer = new Timer();
		muzzle = GetTree().Root.GetNode<Sprite2D>("main/Player/muzzle"); 
		AddChild(muzzleTimer);
		muzzleTimer.WaitTime = 0.1f;
		muzzleTimer.OneShot = true;
		muzzleTimer.Timeout += () => muzzle.Visible = false;
	}

	public void SpawnBullet(int spread)
	{
		RandomNumberGenerator rnd = new RandomNumberGenerator();
		rnd.Randomize();

		RigidBody2D bullet = bulletScene.Instantiate<RigidBody2D>();
		
		// Направление пули с учетом разброса
		float spreadAngle = (float)rnd.RandfRange(-spread, spread) / 100;
		Vector2 direction = new Vector2((float)Math.Cos(GlobalRotation), (float)Math.Sin(GlobalRotation));
		direction = direction.Rotated(spreadAngle);
		
		// Устанавливаем позицию, поворот и свойства пули
		bullet.GlobalPosition = muzzle.GlobalPosition;
		bullet.Rotation = GlobalRotation + spreadAngle;
		bullet.Set("speed", bulletSpeed);
		bullet.Set("damage", bulletDamage);

		GetTree().Root.AddChild(bullet);
	}

	public override void _PhysicsProcess(double delta)
	{
		bool notAutomatic = Input.IsActionJustPressed("shoot");
		bool automatic = Input.IsActionPressed("shoot");
		bool aim = Input.IsActionPressed("aim");
		bool run = Input.IsActionJustPressed("run");

		// Рассчитываем разброс
		int totalSpread = spread;
		if (aim)
		{
			totalSpread = 0;
		}
		else if (run)
		{
			totalSpread = spread * 3;
		}
		else
		{
			totalSpread = spread + Convert.ToInt16(spread * 0.75);
		}

		// Реализация стрельбы
		if ((isAuto ? automatic : notAutomatic) && timeUntilFire > fireRate)
		{
			for (int i = 0; i < bulletsPerShoot; i++)
			{
				SpawnBullet(totalSpread);
				muzzle.Visible = true;
				muzzleTimer.Start();
			}
			timeUntilFire = 0f;
		}
		else
		{
			timeUntilFire += (float)delta;
		}
	}
}
