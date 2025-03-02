using Godot;
using System;
using System.Threading.Tasks;

public partial class LabelHolder : Node2D
{
	private Label label;
	private ColorRect backGround;
	private bool isTyping = false;

	public override void _Ready()
	{
		label = GetNode<Label>("Label");
		backGround = GetNode<ColorRect>("BackGround");
		backGround.Visible = false;
		label.Visible = false;
	}

	public override void _Process(double delta)
	{
		Rotation = -GetParent<Node2D>().Rotation;
	}

	public async void ShowText(string text, float totalDisplayTime)
	{
		if (isTyping)
			return;

		isTyping = true;
		backGround.Visible = true;
		label.Visible = true;

		var typingSpeed = totalDisplayTime / text.Length;

		foreach (var ch in text)
		{
			label.Text += ch;
			await ToSignal(GetTree().CreateTimer(typingSpeed), "timeout");
		}

		await ToSignal(GetTree().CreateTimer(totalDisplayTime), "timeout");
		label.Visible = false;
		backGround.Visible = false;
		label.Text = "";
		isTyping = false;

	}
}
