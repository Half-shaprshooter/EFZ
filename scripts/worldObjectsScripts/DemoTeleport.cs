using Godot;
using System;

public partial class DemoTeleport : TextureButton
{
	[Export] private TextureButton _anotherTextureButton;
	
	public void OnButton1Pressed()
	{
		GD.Print("Button 2 pressed");
		var targetNode = GetNode("/root/main/Player");
		targetNode.Call("TransportDemo");
		_anotherTextureButton.QueueFree();
		QueueFree(); // Удаляем сцену
	}
}
