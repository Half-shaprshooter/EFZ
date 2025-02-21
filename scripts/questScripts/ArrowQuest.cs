using Godot;
using System;

public partial class ArrowQuest : Sprite2D
{
	private Vector2 PosForAngle;
	private Vector2 Target;
	
	public override void _Ready()
	{
		PosForAngle = Quest.globalPos;
	}
	
	public override void _Process(double delta)
	{
		PosForAngle = Quest.globalPos;
		Target = (PosForAngle - PlayerControl.globalPos).Normalized();
		
		var targetRotation = (PosForAngle - PlayerControl.globalPos).Angle();
		Rotation = (float)Mathf.LerpAngle(Rotation, targetRotation, 10 * delta);
	}
}
