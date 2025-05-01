using Godot;
using System;
using System.Threading.Tasks;

public partial class Remarks : Node2D
{
	//ссылка на персонажа, к которому будет привязана нода Remarks
	[Export] CharacterBody2D body;

	private bool isPlayerNear = false; //если игрок входит в зону, то true

	private Area2D proximityArea; //зона в которой бот обнаруживает игрока
	private RayCast2D lookRay; //луч ,куда смотрит бот

	//ноды текста и заднего фона текста
	private Label label;
	private ColorRect backGround;

	private bool isTyping = false; 
	private bool isHeSay = false; //если сказал, то true, чтобы без остановки не печатались фразы
	private bool isHeSee = false;

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

	public override void _Ready()
	{
		lookRay = GetNode<RayCast2D>("LookRay");

		label = GetNode<Label>("Label");
		backGround = GetNode<ColorRect>("BackGround");
		backGround.Visible = false;
		label.Visible = false;

		proximityArea = GetNode<Area2D>("ProximityArea");
		proximityArea.Visible = true;
		proximityArea.BodyEntered += OnBodyEntered;
		proximityArea.BodyExited += OnBodyExited;
	}

	public override void _Process(double delta)
	{
		Rotation = -GetParent<Node2D>().Rotation;

		if (isPlayerNear && !isHeSay && isHeSee)
		{
			SayRemark("friend");
			isHeSay = true;
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
		proximityArea.Rotation = body.Rotation;
	}


	//метод что печатает рандомную фразу исходя из отношений и дистанции
	public void SayRemark(string relation, float time = 3f)
	{
		if (playerBody == null || !thoughtsByRelationAndDistance.ContainsKey(relation))
			return;

		byte distanceCategory = GetDistanceCategory(playerBody);

		var distanceDict = thoughtsByRelationAndDistance[relation];

		if (distanceDict.ContainsKey(distanceCategory))
		{
			var phrases = distanceDict[distanceCategory];
			var phrase = phrases[random.Next(phrases.Count)];
			TriggerRemark(phrase, time);
		}
	}


	//метод для вызова метода печатания текста(чтобы не мучаться с await в других местах)
	public async void TriggerRemark(string message, float time = 3f)
	{
		await ShowText(message, time);
	}

	//метод печатания текста
	public async Task ShowText(string text, float totalDisplayTime)
	{
		if (isTyping)
			return;

		isTyping = true;
		backGround.Visible = true;
		label.Visible = true;
		label.Text = "";

		var typingSpeed = totalDisplayTime / text.Length;

		foreach (var ch in text)
		{			
			label.Text += ch;
			await ToSignal(GetTree().CreateTimer(typingSpeed), "timeout");
		}

		await ToSignal(GetTree().CreateTimer(totalDisplayTime), "timeout");
		label.Visible = false;
		backGround.Visible = false;
		label.Text = "";
		isTyping = false;
	}

	//даёт информацию если игрок зашёл в зону видимости npc
	private void OnBodyEntered(Node2D body)
	{
		if (body is PlayerControl)
		{
			playerBody = body;
			isPlayerNear = true;
		}
	}

	//даёт информацию если игрок вышел из зоны видимости npc
	private void OnBodyExited(Node2D body)
	{
		if (body is PlayerControl)
		{
			playerBody = null;
			isPlayerNear = false;
			isHeSay = false;
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
