using Godot;
using System;

public partial class ArrowQuest : Sprite2D
{
	private List<Quest> _quests = new List<Quest>();
	private Quest _quest;
	private String questText;
	private Vector2 PosForAngle;
	private Vector2 Target;
	private QuestList questList;
	
	public override void _Ready()
	{
		questList = GetNode<QuestList>("/root/main/CanvasLayer/QuesList");
	}
	
	public override void _Process(double delta)
	{
		if (_quest == null || !IsInstanceValid(_quest)) 
		{
			return;
		}
		PosForAngle = _quest.GlobalPosition;
		Target = (PosForAngle - PlayerControl.globalPos).Normalized();
		
		var targetRotation = (PosForAngle - PlayerControl.globalPos).Angle();
		Rotation = (float)Mathf.LerpAngle(Rotation, targetRotation, 10 * delta);
	}

	public void addQuest(Quest quest, String text)
	{
		_quest = quest;
		questText = text;
	}

	public void PlayerEnterQuestArea(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			//Пока так надо
			// try
			// {
			String text = questText;
			if (QuestList.Instance.HaveQuest(text))
			{
				var quest = _quest;
				QuestList.Instance.AddQuest(_quest.questText,_quest.nextQuest);
				quest.die();
				QuestList.Instance.RemoveQuest(text, _quest);
				GD.Print("Меняю квест");
			}
			// }catch(Exception e){}
		}
	}
}
