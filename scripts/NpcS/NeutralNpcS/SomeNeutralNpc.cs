public partial class SomeNeutralNpc : TalkableNpc
{
	private Label _label;
	private static List<NpcDialogue> _npcDialogues;
	public override void _Ready()
	{
		NpcName = "TestNeutralNpcName";
		InterfaceSelectionObject interSelect = new InterfaceSelectionObject(1, "Вариант 1");
		InterfaceSelectionObject interSelect2 = new InterfaceSelectionObject(2, "Вариант 2");
		InterfaceSelectionObject interSelect3 = new InterfaceSelectionObject(-1, "Вариант 3");
		_npcDialogues = new List<NpcDialogue>
		{
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect, interSelect2}, "Вариант текста 1", 0),
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect3}, "Вариант текста 2", 1),
			new NpcDialogue(new List<InterfaceSelectionObject>(){interSelect3}, "Вариант текста 3", 2)
		};
		Health = 100;
		Hostility = Hostility.Neutral;
		_label = GetNode<Label>("ButtonText");
	}

	public override void _Process(double delta)
	{
		if (PlayerNear)
		{
			_label.Visible = true;
		}
		else
		{
			_label.Visible = false;
		}
	}

	public void SetDialog()
	{
		InterfaceManager.dialogueManager.NpcDialogues = _npcDialogues;
		InterfaceManager.dialogueManager.DialogHeader = NpcName;
	}
	private void OnDetectionAreaBodyEntered(Node2D body)
	{
		if (body is PlayerControl)
		{
			PlayerNear = true;
		}
	}

	private void OnDetectionAreaBodyExited(Node2D body)
	{
		if (body is PlayerControl)
		{
			PlayerNear = false;
		}
	}
}
