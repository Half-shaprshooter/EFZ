using Godot;
using System;
using System.Threading.Tasks;

public partial class Remarks : Node2D
{
	//ссылка на персонажа, к которому будет привязана нода Remarks
	[Export] CharacterBody2D body;

	private bool isPlayerInVisible = false; //true - если игрок в зоне видимости.
	private bool isPlayerNear = false;

	private Area2D proximityArea; //зона в которой бот обнаруживает игрока
	private Area2D playerNearArea; //зона, которая определяет, находится ли игрок рядом
	private RayCast2D lookRay; //луч ,куда смотрит бот

	//ноды текста и заднего фона текста
    private Label label;
	private ColorRect backGround;

	private bool isTyping = false; //если печатается текст - true
	private bool isHeSay = false; //если сказал, то true, чтобы без остановки не печатались фразы
	private bool isHeSee = false; //если бот видит игрока - true
	
	private Timer cooldownTimer; // Таймер для задержки между фразами
	private bool isOnCooldown = false; // Флаг, что идёт задержка

	private bool IsPriorityPhrase => isPlayerInVisible && isHeSee;
	private bool isSayingPriorityPhrase = false;
	private bool isHeSayPriorityPhrase = false;

	private float minPriorityDelay = 2f; // Минимум 2 сек между важными фразами
	private float timeSinceLastPriority = 0f;

	private Random random = new Random();

	private Node2D playerBody;
	private Vector2 playerPos;

	// Дистанции: 0 = близко, 1 = средне, 2 = далеко
	private Dictionary<string, Dictionary<byte, List<string>>> thoughtsByRelationAndDistance = new Dictionary<string, Dictionary<byte, List<string>>>
	{
		{ "friend", new Dictionary<byte, List<string>>
			{
				{ 0, new List<string> {
					"Ты прям под боком.",
					"Снова ты. Как всегда близко.",
					"Чуть ближе — и ты станешь тенью.",
				}},
				{ 1, new List<string> {
					"Ты недалеко, как обычно.",
					"Хм, держишь дистанцию?",
					"Я тебя вижу, но не слышу.",
				}},
				{ 2, new List<string> {
					"Это ты там, на горизонте?",
					"Далеко, но узнаю походку.",
					"Может подойдёшь ближе?",
				}},
			}
		},
		{ "neutral", new Dictionary<byte, List<string>>
			{
				{ 0, new List<string> {
					"Зачем ты так близко?",
					"Ты вторгаешься в моё пространство.",
					"Слишком близко для незнакомца.",
				}},
				{ 1, new List<string> {
					"Держишь дистанцию. Умно.",
					"Следишь за мной?",
					"Кто ты такой вообще?",
				}},
				{ 2, new List<string> {
					"Просто прохожий?",
					"Стоишь там и смотришь...",
					"Кажется, кто-то наблюдает.",
				}},
			}
		},
		{ "enemy", new Dictionary<byte, List<string>>
			{
				{ 0, new List<string> {
					"Ты слишком близко... опасно близко.",
					"Ещё шаг — и ты пожалеешь.",
					"Я тебя уничтожу.",
				}},
				{ 1, new List<string> {
					"Ты ещё жив, удивительно.",
					"Смотришь издалека, трус?",
					"Можешь подойти, если не боишься.",
				}},
				{ 2, new List<string> {
					"Даже издалека ты раздражаешь.",
					"Надеюсь, ветер принесёт твою погибель.",
					"Спрятался? Жалко.",
				}},
			}
		}
	};

	private List<string> idleThoughts = new List<string>
	{
		"Интересно, откуда этот скрипучий звук?..",
		"Постой-ка... это ветер или шаги?",
		"Мне кажется, кто-то рядом.",
		"Что за тишина?.. напрягает.",
		"Какое странное ощущение... будто кто-то наблюдает.",
		"Иногда молчание громче слов.",
		"Если бы я мог читать мысли... хотя лучше не надо.",
	};

	public override void _Ready()
	{
		lookRay = GetNode<RayCast2D>("LookRay");

		label = GetNode<Label>("Label");
		backGround = GetNode<ColorRect>("BackGround");
		backGround.Visible = false;
		label.Visible = false;

		proximityArea = GetNode<Area2D>("ProximityArea");
		proximityArea.Visible = true;

		playerNearArea = GetNode<Area2D>("PlayerNearArea");
		playerNearArea.Visible = true;

		label.Visible = true;

		// Создаём и настраиваем таймер
		cooldownTimer = new Timer();
		AddChild(cooldownTimer);
		cooldownTimer.Timeout += OnCooldownEnded;
	}

	public override void _Process(double delta)
	{
		Rotation = -GetParent<Node2D>().Rotation;
		
		timeSinceLastPriority += (float)delta; // Обновляем таймер

		// Если игрок виден, но не прошло minPriorityDelay — игнорируем
		bool canSayPriority = timeSinceLastPriority >= minPriorityDelay;

		if (isPlayerInVisible && isHeSee && canSayPriority && !isHeSayPriorityPhrase)
		{
			SayRemark("friend", isPriority: true);
			timeSinceLastPriority = 0f; // Сброс таймера
		}
		else if (isPlayerNear && !isHeSee && !isOnCooldown && !isHeSay)
		{
			SayRemark();
		}

		if (playerBody != null)
		{
			playerPos = playerBody.Position;
		}
	}

    public override void _PhysicsProcess(double delta)
    {
		if (playerBody != null)
		{
			isHeSee = BuildRayToTarget(playerBody);
		}
		else
		{
			isHeSee = false;
		}
		
		proximityArea.Rotation = body.Rotation;
    }


	//метод что печатает рандомную фразу исходя из отношений и дистанции
	public void SayRemark(string relation, bool isPriority)
	{
		// Если это НЕ приоритетная фраза, и идёт кулдаун/уже говорим — выходим
    	if (!isPriority && (isOnCooldown || isHeSay))
        	return;

		// Если это приоритет, но УЖЕ говорим приоритет — не перебиваем
    	if (isPriority && isSayingPriorityPhrase)
        	return;
		
		// Прерываем кулдаун только для новых приоритетных фраз
		if (isPriority && (isOnCooldown || isHeSay))
			InterruptCooldown();

		byte distanceCategory = GetDistanceCategory(playerBody);

		var distanceDict = thoughtsByRelationAndDistance[relation];

		if (distanceDict.ContainsKey(distanceCategory))
		{
			var phrases = distanceDict[distanceCategory];
			var phrase = phrases[random.Next(phrases.Count)];
			isHeSayPriorityPhrase = true;
			label.Text = phrase;

			StartCooldown(phrase); // Запускаем таймер
        	isHeSay = true;
		}
	}

	//метод, что печатает случайную фразу, если игрок просто рядом
	public void SayRemark()
	{
		if (isOnCooldown || idleThoughts.Count == 0) return;

		var phrase = idleThoughts[random.Next(idleThoughts.Count)];
		label.Text = phrase;
		
		StartCooldown(phrase); // Запускаем таймер
    	isHeSay = true;
	}

	private void StartCooldown(string text)
	{
		if (string.IsNullOrEmpty(text)) 
			return;

		// Вычисляем длительность: 0.2 сек * кол-во символов
		float duration = 0.2f * text.Length;
		isOnCooldown = true;
		cooldownTimer.Start(duration);
	}

	private void OnCooldownEnded()
	{
		isOnCooldown = false;
		isHeSay = false;
		isSayingPriorityPhrase = false;
	}

	private void InterruptCooldown()
	{
		if (!cooldownTimer.IsStopped())
		{
			cooldownTimer.Stop();
			isOnCooldown = false;
			isHeSay = false;
		}
	}

	//даёт информацию если игрок зашёл в зону видимости npc
	private void VisualRangeEnter(Node2D body)
	{
    	if (body is PlayerControl)
    	{
			playerBody = body;
        	isPlayerInVisible = true;
		}
	}

	//даёт информацию если игрок вышел из зоны видимости npc
	private void VisualRangeExited(Node2D body)
	{
    	if (body is PlayerControl)
    	{
			playerBody = null;
        	isPlayerInVisible = false;
			isHeSay = false;
			isHeSayPriorityPhrase = false;
    	}
	}

	//даёт информацию, если игрок рядом
	private void PlayerNearEntered(Node2D body)
	{
		if (body is PlayerControl)
    	{
			isPlayerNear = true;
    	}
	}

	//даёт информацию, если игрок вышел из близкой зоны.
	private void PlayerNearExited(Node2D body)
	{
		if (body is PlayerControl)
    	{
			isPlayerNear = false;
    	}
	}	

	//возвращает true, если удалось построить луч для игрока(т.е. в прямой видимости)
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

	//возвращает дистанцию до цели(0 - близко, 1 - средне, 2 - далеко)
	private byte GetDistanceCategory(Node2D target)
	{
		if (target == null) return 255;
		var distance = body.GlobalPosition.DistanceTo(target.GlobalPosition);
		GD.Print(distance);
		if (distance < 400f)
			return 0;
		else if (distance < 1000f)
			return 1;
		else
			return 2;
	}
}