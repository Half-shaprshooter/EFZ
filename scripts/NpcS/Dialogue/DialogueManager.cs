using System.Linq;

public  partial class DialogueManager : Control
{
	public static bool IsTradeIndex;
	public static bool TradeIndexActivated;
	public List<NpcDialogue> NpcDialogues;
	public List<InterfaceSelection> Selections = new List<InterfaceSelection>();
	
	private bool _isDialogueUp;
	
	private int _currentIndex = 0;

	public string DialogHeader;
	
	[Export] public PackedScene InterfaceSelectableObject;
	
	public override void _Ready()
	{
		GetNode<Panel>("Panel").Hide();
		GetNode<TextureRect>("Fon").Hide();
	}

	public override void _Process(double delta)
	{
		if (_isDialogueUp)
		{
			//GetTree().Paused = true;
			Engine.TimeScale = 0.0f;
			if (Input.IsActionJustPressed("ui_left"))
			{
				foreach (var item in Selections)
				{
					item.SetSelected(false);
				}
				_currentIndex -= 1;
				if (_currentIndex < 0)
				{
					_currentIndex = 0;
				}
				Selections[_currentIndex].SetSelected(true);
			}
			else if(Input.IsActionJustPressed("ui_right"))
			{
				foreach (var item in Selections)
				{
					item.SetSelected(false);
				}
				_currentIndex += 1;
				if (_currentIndex > Selections.Count - 1)
				{
					_currentIndex = Selections.Count-1;
				}
				Selections[_currentIndex].SetSelected(true);
			}
			else if (Input.IsActionJustPressed("accept_dialog"))
			{
				DisplayNextDialogueElement(Selections[_currentIndex].interfaceSelectionObject.SelectionIndex);
			}
		}
	}
	
	public void ShowDialougeElement()
	{
		GetNode<Panel>("Panel").Show();
		GetNode<TextureRect>("Fon").Show();
		GetNode<Label>("Panel/Label").Text = DialogHeader;
		Write(NpcDialogues[0]);
	}

	public void Write(NpcDialogue dialogue)
	{
		foreach (Node item in GetNode<Node>("Panel/HBoxContainer").GetChildren())
		{
			item.QueueFree();
		}
		Selections = new List<InterfaceSelection>();
		GetNode<RichTextLabel>("Panel/RichTextLabel").Text = dialogue.DisplayText;
		
		foreach (var item in dialogue.InterfaceSelectionObjects)
		{
			InterfaceSelection interfaceSelection = InterfaceSelectableObject.Instantiate() as InterfaceSelection;
			interfaceSelection.interfaceSelectionObject = item;
			GetNode<HBoxContainer>("Panel/HBoxContainer").AddChild(interfaceSelection); 
			Selections.Add(interfaceSelection);
			
			interfaceSelection.SetSelected(false);
		}
		Selections[0].SetSelected(true);
		_currentIndex = 0;
		_isDialogueUp = true;
	}

	private void ShutDownDialogue()
	{
		GetNode<Panel>("Panel").Hide();
		GetNode<TextureRect>("Fon").Hide();
		Engine.TimeScale = 1.0f;
		//GetTree().Paused = false;
		_isDialogueUp = false;
	}

	private void DisplayNextDialogueElement(int index)
	{
		if (NpcDialogues.ElementAtOrDefault(index) == null || index == -1)
		{
			if (index == -1 && TradeIndexActivated)
			{
				IsTradeIndex = true;
				TradeIndexActivated = false;
			}
			ShutDownDialogue();
			//NpcDialogues.RemoveRange();
		}
		else
		{
			if (index == 1)
			{
				TradeIndexActivated = true;
			}
			Write(NpcDialogues[index]);
		}
	}
}
