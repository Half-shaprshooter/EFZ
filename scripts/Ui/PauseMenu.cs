using Godot;
using System;

public partial class PauseMenu : Control
{
	public override void _Ready()
	{
		Visible = false;
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("pause") && GetTree().Paused == false)
		{
			Pause();
		}else if (Input.IsActionJustPressed("pause") && GetTree().Paused == true)
		{
			Resume();
		}
	}

	public void Resume()
	{
		GetTree().Paused = false; 
		Visible = false;
	}

	public void Pause()
	{
		GetTree().Paused = true;
		Visible = true;
	}

	public void OnResumePressed()
	{
		Resume();
	}
	//TODO: Когда будут реализованы сохранения, сюда добавить
	public void OnQuitPressed()
	{
		GetTree().Quit();
	}

	//TODO: Тестовая логика, потом под переделку
	public void OnRestartPressed()
	{
		GetTree().ReloadCurrentScene();
	}
}
