namespace EscapeFromZone.scripts.FriendlyNpcS;

public partial class Mitya : NPC_AI
{
    private Label _label;
	private static List<NpcDialogue> _npcDialogues;
	public bool CanTalk;
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
		
		_label = GetNode<Label>("ButtonText");
		base._Ready();
	}

	protected override void HandlePatrol()
	{
		if (!CanTalk)
		{
			base.HandlePatrol();
		}
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