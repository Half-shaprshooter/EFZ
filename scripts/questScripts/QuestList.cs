using Godot;
using System;
using System.Collections.Generic;

public partial class QuestList : VBoxContainer
{
	private List<string> questList = new List<string>();

	public override void _Ready()
	{
		AddQuest("Выберись из этого места");
		AddQuest("Вылечи ногу");
	}

	// Добавляет задание
	public void AddQuest(string text)
	{
		questList.Add(text);
		var label = new Label
		{
			Text = text
		};
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
