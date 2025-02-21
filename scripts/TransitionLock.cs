using Godot;
using System;

public partial class TransitionLock : Area2D
{
	private bool entered;
	private Label _label;
	public override void _Ready()
	{
		_label = GetNode<Label>("Guide");
	}
	
	public override void _Process(double delta)
	{
		if (entered)
		{
			_label.Visible = true;
		}
		else
		{
			_label.Visible = false;
		}

		if (Input.IsActionJustPressed("next_scene") && entered)
		{
			GetTree().ChangeSceneToFile("res://Scenes/secondScene.tscn");
		}
	}

	public void OnTransitionBodyEntered(Node2D body)
	{
		if (body is PlayerControl)
		{
			entered = true;
		}
	}

	public void OnTransitionBodyExited(Node2D body)
	{
		if (body is PlayerControl)
		{
			entered = false;
		}
	}
}
