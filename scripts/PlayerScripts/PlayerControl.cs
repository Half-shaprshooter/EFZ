using System.Diagnostics;
using EscapeFromZone.scripts.PlayerScripts;

public partial class PlayerControl : CharacterBody2D
{
	//TODO: БАГ С ПАУЗОЙ НА ВРАГА 
	//TODO: УКПАСТЬ КОД У ГУФА ГИТ
	public FireTypeImpl fireTypeInHands;
	private AnimatedSprite2D _walk;
	private AnimatedSprite2D _legs;
	private AnimationPlayer _wound;

	private Slots _slots;
	private float Speed { get; set; }
	private bool _isInGrass;
	private int _damageToPlayer;
	private float _rotationSpeed = 6f;
	private int _health;
	private int _maxHealth;

	private double _stamina;
	private int _staminaCons;
	private int _maxStamina;

	private bool _isWalking;
	private bool _isRunning;
	private bool _isStading;

	private Texture _aimCursor = (Texture)GD.Load("res://sprites/cross/aimCross.png");
	private Texture _notAimCursor = (Texture)GD.Load("res://sprites/cross/notAimCross.png");
	private Texture _runCursor = (Texture)GD.Load("res://sprites/cross/runCross.png");

	private Vector2 _hotspot8 = new Vector2(8, 8);
	private Vector2 _hotspot16 = new Vector2(16, 16);
	private Vector2 _hotspot32 = new Vector2(32, 32);

	public static Vector2 globalPos;
	public static Vector2 localPos;
	
	private double _idleTimer = 0f;
	private bool _hasPlayedIdleAnim = false;
	private const double IDLE_TRIGGER_TIME = 5;

	public PlayerControl()
	{
		//Инициализация оружия в руках, изначально милишка, будет использоваться вместе с активными слотами инвентаря
		fireTypeInHands = this.GetNodeOrNull<FireTypeImpl>("FireTypeImpl");

		if (fireTypeInHands == null)
		{
			fireTypeInHands = new FireTypeImpl();
			this.AddChild(fireTypeInHands);
			fireTypeInHands.Name = "FireTypeImpl";
		}

		fireTypeInHands.fireType = FireType.Melee;
	}

	public override void _Ready()
	{
		_walk = GetNode<AnimatedSprite2D>("PlayerAnimation");
		_legs = GetNode<AnimatedSprite2D>("LegsAnimation");
		_wound = GetNode<AnimationPlayer>("PointLight2D/AnimationPlayer");
		_health = PlayerData.PlayerHealth;
		_maxHealth = PlayerData.PlayerMaxHealth;
		_stamina = PlayerData.PlayerStamina;
		_staminaCons = PlayerData.staminaConsumption;
		_maxStamina = PlayerData.PlayerMaxStamina;
		globalPos = this.GlobalPosition;
	}

	//Необходим для проверки Body на плеера в сигналах
	public void Player()
	{
	}

	private void _on_grass_2_grass_exited()
	{
		_isInGrass = false;
	}

	private void _on_grass_2_grass_entered()
	{
		_isInGrass = true;
	}

	public override void _Process(double delta)
	{
		globalPos = this.GlobalPosition;
		localPos = this.Position;

		PlayerData.PlayerHealth = _health;
		PlayerData.PlayerStamina = _stamina;
		
		_isStading = true;
		
		Dialogue();
		GrassLogic();
		MovementAndAnimLogic(delta);
		//Логика слотов оружия
		GunSlotsLogic();

		if (_health < (double)_maxHealth * 0.3)
		{
			_wound.Play("wound");
		}

		StaminaRecovery(delta);
	}
	
	private void GunSlotsLogic()
	{
		if (Input.IsActionJustPressed("firstSlot"))
		{
			fireTypeInHands.fireType = FireType.Melee;
			GD.Print("Текущее оружие ближнего боя");
			_slots = Slots.HANDS;
		}

		if (Input.IsActionJustPressed("secondSlot"))
		{
			GD.Print("Текущее оружие дальнего боя");
			fireTypeInHands.fireType = FireType.FireArm;
			_slots = Slots.AUTOMATIC;
		}
	}

	private void MovementAndAnimLogic(double delta)
	{
		float totalSpeed;
		var mousePosition = GetGlobalMousePosition();
		var targetRotation = (mousePosition - GlobalPosition).Angle();
		var moveInput = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		
		if (!Input.IsActionPressed("run") || (Input.IsActionPressed("run") && Input.IsActionPressed("aim")))
		{
			Rotation = (float)Mathf.LerpAngle(Rotation, targetRotation, _rotationSpeed * delta);
		}
		if (Input.IsActionPressed("aim"))
		{
			_walk.Play("ScopePistol");
			Input.SetCustomMouseCursor(_aimCursor, Input.CursorShape.Arrow, _hotspot16);
			totalSpeed = (float)(Speed * 0.50);
			_legs.Play("WalkLegs");
	
			if (!Input.IsActionPressed("move_down") && !Input.IsActionPressed("move_left") &&
				!Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_up")){
				_legs.Stop();
			}
		}
		else if (Input.IsActionPressed("run") && _stamina > 0)
		{
			switch (_slots)
			{
				case Slots.HANDS:
					_walk.Play("Run");
					_legs.Stop();
					break;
				case Slots.KNIFE:
					_walk.Play("RunWithKnife");
					_legs.Stop();
					break;
				case Slots.AUTOMATIC:
					_walk.Play("RunWithAr");
					_legs.Stop();
					break;
				case Slots.PISTOL:
					_walk.Play("RunWithPistol");
					_legs.Stop();
					break;
			}
			
			_stamina -= _staminaCons * delta;
			_stamina = Math.Clamp(_stamina, 0f, _maxStamina);
			Input.SetCustomMouseCursor(_runCursor, Input.CursorShape.Arrow, _hotspot8);
			totalSpeed = (float)(Speed * 2);
			Rotation = (float)Mathf.LerpAngle(Rotation, moveInput.Angle(), _rotationSpeed * delta);
		}
		else
		{
			totalSpeed = Speed;
			Input.SetCustomMouseCursor(_notAimCursor, Input.CursorShape.Arrow, _hotspot32);
			if (Input.IsActionPressed("move_down") || Input.IsActionPressed("move_left") ||
				Input.IsActionPressed("move_right") ||
				Input.IsActionPressed("move_up") && !Input.IsKeyPressed(Key.Shift))
			{
				_walk.Play("Walk");
				_legs.Play("WalkLegs");
			}
			if (!Input.IsActionPressed("move_down") && !Input.IsActionPressed("move_left") &&
				!Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_up"))
			{
				_idleTimer += delta;
				//Баг, должно проигрываться 1 раз, но проигрывается без основки
				if (_idleTimer >= IDLE_TRIGGER_TIME && !_hasPlayedIdleAnim)
				{
					_walk.Play("newStand");
					_hasPlayedIdleAnim = true;
				}
				else if(_idleTimer <= IDLE_TRIGGER_TIME)
				{
					switch (_slots)
					{
						case Slots.HANDS:
							_walk.Play("Stand");
							break;
						case Slots.KNIFE:
							_walk.Play("StandWithKnife");
							break;
						case Slots.AUTOMATIC:
							_walk.Play("StandWithAr");
							break;
						case Slots.PISTOL:
							_walk.Play("StandWithPistol");
							break;
					}
					_legs.Stop();
				}
			}
			else
			{
				_hasPlayedIdleAnim = false;
				_idleTimer = 0;
			}
			
		}
		Velocity = moveInput * totalSpeed;
		MoveAndSlide();
	}

	public bool NotMove()
	{
		if (!Input.IsActionPressed("move_down") && !Input.IsActionPressed("move_left") &&
			!Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_up"))
		{
			return true;
		}

		return false;
	}
	
	private void GrassLogic()
	{
		if (_isInGrass)
		{
			Speed = 300;
		}

		if (!_isInGrass)
		{
			Speed = 600;
		}
	}

	private void Dialogue()
	{
		if (Input.IsActionJustPressed("dialogue_button") && GetNode<RayCast2D>("CenterRaycast").IsColliding())
		{
			var obj = (Node2D)GetNode<RayCast2D>("CenterRaycast").GetCollider();
			if (obj is SomeNeutralNpc)
			{
				SomeNeutralNpc npc = obj as SomeNeutralNpc;
				npc.SetDialog();
				InterfaceManager.dialogueManager.ShowDialougeElement();
			}

			if (obj is Seller)
			{
				Seller npc = obj as Seller;
				npc.SetDialog();
				InterfaceManager.dialogueManager.ShowDialougeElement();
			}
		}
	}

	public void TakeDmg(int dmg)
	{
		if (_health > 0)
		{
			_health -= dmg;

			if (_health < 0)
			{
				_health = 0;
			}

			_wound.Play("woundx3");
		}
		
		else
		{
			GD.Print("Увы");
		}
	}

	//метод восстановления выносливости
	private void StaminaRecovery(double deltaTime)
	{
		if (!Input.IsActionPressed("run") && _stamina < 100)
		{
			_stamina += 7 * deltaTime;
			_stamina = Math.Clamp(_stamina, 0f, _maxStamina);
		}
	}
}
