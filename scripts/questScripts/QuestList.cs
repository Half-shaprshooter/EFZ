using Godot;
using System;
using System.Collections.Generic;

public partial class QuestList : VBoxContainer
{
	public static QuestList Instance { get; private set; }
	private List<string> questList = new List<string>();
	private ArrowQuest ArrowQuest;
	
	public override void _Ready()
	{
		Instance = this;
		ArrowQuest = GetNode<ArrowQuest>("/root/main/CanvasLayer/SubViewportContainer/ArrowQuest");
		AddQuest("Вернуться к церкви", GetNode<Quest>("/root/main/NavigationRegion2D/House6/TriggerForQuest"));
	}

	// Добавляет задание
	public void AddQuest(string text, Quest quest)
	{
		questList.Add(text);
		var label = new Label
		{
			Text = text
		};
		ArrowQuest.addQuest(quest, text);
		AddChild(label);
	}
	
	// Удаляет задание
	public void RemoveQuest(string text)
	{
		if (questList.Remove(text))
		{
			foreach (var child in GetChildren())
			{
				if (child is Label label && label.Text == text)
				{
					label.QueueFree();
				}
			}
		}
	}

	public bool HaveQuest(string text) => questList.Contains(text);
	
	// Удаляет все задания
	public void ClearQuestList()
	{
		questList.Clear();
		foreach (var child in GetChildren())
		{
			if (child is Label)
				child.QueueFree();
		}
	}
}
