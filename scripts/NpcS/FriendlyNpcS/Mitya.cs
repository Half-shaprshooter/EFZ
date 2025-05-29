namespace EscapeFromZone.scripts.FriendlyNpcS;

public partial class Mitya : NPC_AI
{
	private Label _label;
	private static List<NpcDialogue> _npcDialogues;
	private static List<NpcDialogue> _npcDialogues2;
	public bool CanTalk;
	private int _patrolPointsVisited = 0;
	private int _dialogueNumber = 0;
	[Export] public Node2D[] NextPatrolPoints;
	[Export] public Random Random1 { get; set; }
	[Export] public Random Random2 { get; set; }

	public override void _Ready()
	{
		NpcName = "Mitya";
		CanTalk = true;
		InterfaceSelectionObject interSelect = new InterfaceSelectionObject(2, "Здарова, да");
		InterfaceSelectionObject interSelect2 = new InterfaceSelectionObject(3, "Ходили туда?");
		InterfaceSelectionObject interSelect3 = new InterfaceSelectionObject(4, "Идём?");
		InterfaceSelectionObject interSelect4 = new InterfaceSelectionObject(-1, "Ага");
		
		InterfaceSelectionObject interSelect5 = new InterfaceSelectionObject(-1, "Отлично");
		
		_npcDialogues = new List<NpcDialogue>
		{
			new NpcDialogue(
				new List<InterfaceSelectionObject>(){interSelect}, 
				"Ты, новенький, от Лыткина? Здорова.", 0, true),
			new NpcDialogue(
				new List<InterfaceSelectionObject>(){interSelect}, 
				"Ты, новенький, от Лыткина? Здорова.", 0, true),
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect2}, 
				"Лыткин, значит. Ну, если он одобрил — валяй, расскажу." +
				"\n Есть старая церковь, километра три отсюда. Когда-то там жили, молились..." +
				"\n А потом - тишина. Ни связи, ни сталкеров, ни писка. Только пустота и серая зона на карте." +
				"\n Нужно проверить. Может, найдём что ценное... а может - кого-то.", 2, true),
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect3}, 
				"Несколько раз. Вернулись не все. Один говорил, будто слышал чей-то рёв. Другой - что видел человека без лица." +
				"\nНо, честно, тут и не такое бывает. Главное - не отходи. Если начнётся - держим строй и валим.", 3, true),
			new NpcDialogue(
				new List<InterfaceSelectionObject>(){interSelect4}, 
				"Да, не будем терять время", 4, true),
		};

		_npcDialogues2 = new List<NpcDialogue>()
		{
			new NpcDialogue(
				new List<InterfaceSelectionObject>(){interSelect5}, 
				"Чёрт бы его..." +
				"\n Слушай, нужно, прочесать всю церковь. От подвала до колокольни." +
				"\n Если там что-то осталось - мы должны это найти. Мы пойдем первыми.", 0, true),
		};
		
		_label = GetNode<Label>("ButtonText");
		base._Ready();
	}

	public override void _Process(double delta)
	{
		if (_patrolPointsVisited == PatrolPoints.Length && _dialogueNumber == 1)
		{
			_patrolPointsVisited = 0;
			SetVelocityToZero();
			CanTalk = true;
			_npcDialogues = _npcDialogues2;
		}
		base._Process(delta);
	}

	protected override void HandlePatrol()
	{
		if (!CanTalk)
		{
			base.HandlePatrol();
		}
	}

	protected override void AdvancePatrolPoint()
	{
		_patrolPointsVisited++;
		base.AdvancePatrolPoint();
	}
	
	public void SetDialog()
	{
		if (!CanTalk)
			return;
		InterfaceManager.dialogueManager.NpcDialogues = _npcDialogues;
		InterfaceManager.dialogueManager.DialogHeader = NpcName;
		GetTree().Paused = true;
		CanTalk = false;
		Random1.IsStanding = !Random1.IsStanding;
		Random2.IsStanding = !Random2.IsStanding;
		_dialogueNumber++;
		if (_dialogueNumber == 2)
		{
			PatrolPoints = NextPatrolPoints;
		
			// 2. Обновляем массив позиций
			_patrolTargets = new Vector2[PatrolPoints.Length];
			for (int i = 0; i < PatrolPoints.Length; i++)
			{
				_patrolTargets[i] = PatrolPoints[i].GlobalPosition;
			}
		
			// 3. Сбрасываем индекс и счетчик
			_currentPatrolIndex = 0;
			_patrolPointsVisited = 0;
		
			// 4. Запускаем движение к первой точке
			SetNextPatrolPoint();
			Random1.SetNextPoints();
			Random2.SetNextPoints();
		}

	}

	//добавлен async, чтобы работал показ текста
	private void OnDetectionAreaBodyEntered(Node2D body)
	{
		if (body is PlayerControl)
		{
			_label.Visible = true;
			PlayerNear = true;
		}
	}

	private void OnDetectionAreaBodyExited(Node2D body)
	{
		if (body is PlayerControl)
		{
			_label.Visible = false;
			PlayerNear = false;
		}
	}
}
