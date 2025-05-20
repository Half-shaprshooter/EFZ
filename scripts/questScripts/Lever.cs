using Godot;
using System;

public partial class Lever : Node2D
{
	private Label _label;
	private bool isNear;
	private bool once;
	private Kletka _kletka;
	public override void _Ready()
	{
		_label = GetNode<Label>("Label");
		_kletka = GetNode<Kletka>("/root/main/Podval/Kletka4");
	}
	
	public override void _Process(double delta)
	{
		if (isNear && Input.IsActionPressed("interact"))
		{
			_kletka.toggle();
			_label.Visible = false;
			once = true;
		}
	}

	public void _on_area_2d_body_entered(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			if (!once)
			{
				_label.Visible = true;
			}
			isNear = true;
		}
	}

	public void _on_area_2d_body_exited(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			if (!once)
			{
				_label.Visible = false;
			}
			isNear = false;
		}
	}
}
