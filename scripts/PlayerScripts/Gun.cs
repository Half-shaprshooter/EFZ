using EscapeFromZone.scripts.PlayerScripts;

public partial class Gun : Node2D
{	
	private AudioStream AR_SOUND = (AudioStream)GD.Load("res://sounds/effects/ArShootFire.mp3");
	private AudioStream PISTOL_SOUND = (AudioStream)GD.Load("res://sounds/effects/PistolShootFire.mp3");
	
	public FireTypeImpl fireType;
	public FireTypeImpl PlayerFireType;

	private AudioStreamPlayer2D effects;
	private PlayerControl _playerControl;
	[Export] public virtual int spread { get; set; } = 10; // Разброс
	[Export] public virtual bool isAuto { get; set; } = true; // Автоматический режим
	[Export] public virtual float bulletsPerSecond { get; set; } = 5f; // Скорострельность
	[Export] public virtual float bulletSpeed { get; set; } = 10000f; // Скорость пули
	[Export] public virtual float bulletDamage { get; set; } = 30f; // Урон пули
	[Export] public virtual int bulletsPerShoot { get; set; } = 1; // Количество пуль за выстрел

	[Export] PackedScene bulletScene;

	float fireRate;
	float timeUntilFire = 0f;

	private Timer muzzleTimer;
	
	[Export] public Sprite2D muzzle;

	[Export] Node2D whoAttacks = null;

	public Gun()
	{
		fireType = this.GetNodeOrNull<FireTypeImpl>("FireTypeImpl");

		if (fireType == null)
		{
			GD.Print("Добавил");
			fireType = new FireTypeImpl();
			this.AddChild(fireType);
			fireType.Name = "FireTypeImpl";
			GD.Print("Добавил: " + fireType);
		}

		fireType.fireType = FireType.FireArm;
	}

	public override void _Ready()
	{
		effects = GetNode<AudioStreamPlayer2D>("EffectsPlayer");
		_playerControl = GetTree().Root.GetNode<PlayerControl>("main/Player");
		fireRate = 1 / bulletsPerSecond;
		PlayerFireType = GetTree().Root.GetNode<FireTypeImpl>("main/Player/FireTypeImpl"); 
		
		muzzleTimer = new Timer();
		//muzzle = GetTree().Root.GetNode<Sprite2D>("main/Player/muzzle");
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
		bullet.Set("whoAttacks", whoAttacks);

		GetTree().Root.AddChild(bullet);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!PlayerData.CanFire)
		{
			return;
		}
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
		if ((isAuto ? automatic : notAutomatic) && timeUntilFire > fireRate && PlayerFireType.fireType == FireType.FireArm)
		{
			for (int i = 0; i < bulletsPerShoot; i++)
			{
				switch (_playerControl.getCurrentSlot())
				{
					case Slots.AUTOMATIC:
						effects.Stream = AR_SOUND;
						effects.Play();
						break;
					case Slots.PISTOL:
						effects.Stream = PISTOL_SOUND;
						effects.Play();
						break;
				}
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
