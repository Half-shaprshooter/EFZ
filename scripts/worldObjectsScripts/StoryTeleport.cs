using Godot;
using System;

public partial class StoryTeleport : TextureButton
{
	[Export] private TextureButton _anotherTextureButton;
	
	public void OnButton1Pressed()
	{
		GD.Print("Button 1 pressed");
		var targetNode = GetNode("/root/main/Player");
		targetNode.Call("TransportStory");
		_anotherTextureButton.QueueFree();
		QueueFree(); // Удаляем сцену
	}
}
