using Godot;
using System;
using Godot.Collections;

public partial class Note : Node2D
{
	
	private Control _interactUI;
	private bool _playerInArea = false;
	private CanvasLayer _noteUI;
	
	public override void _Ready()
	{
		_interactUI = GetNode<Control>("interactItem");
		_noteUI = GetNode<CanvasLayer>("NoteUI");
		_interactUI.Visible = false;
		_noteUI.Visible = false;
	}

	public override void _Process(double delta)
	{
		if (_playerInArea && Input.IsActionJustPressed("interact") && !_noteUI.Visible)
		{
			_noteUI.Visible = true;
			GetTree().Paused = true;
		}

		if (_noteUI.Visible && Input.IsActionJustPressed("pause"))
		{
			CloseNote();
		}
	}

	private void CloseNote()
	{
		GetTree().Paused = false;
		_noteUI.Visible = false;
	}
	
	private void OnCloseButtonPressed()
	{
		CloseNote();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			_interactUI.Visible = true;
			_playerInArea = true;
		}
	}
	private void OnBodyExited(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			_interactUI.Visible = false;
			_playerInArea = false;
		}
	}
}
