using Godot;
using System;

public partial class PauseMenu : Control
{
	private PanelContainer _panelContainer;
	public bool _pause;
	public override void _Ready()
	{
		_panelContainer = GetNode<PanelContainer>("References");
		Visible = false;
		_panelContainer.Visible = false;
	}
	
	public override void _Process(double delta)
	{
		/*if (Input.IsActionJustPressed("pause") && !GetTree().Paused)
		{
			Pause();
		}
		else if (Input.IsActionJustPressed("pause") && GetTree().Paused)
		{
			Resume();
			_panelContainer.Visible = false;
		}*/
		if (Input.IsActionJustPressed("pause") && Engine.TimeScale == 1.0f)
		{
			Pause();
		}
		else if (Input.IsActionJustPressed("pause") && Engine.TimeScale == 0.0f)
		{
			Resume();
			_panelContainer.Visible = false;
		}
	}

	public void Resume()
	{
		Engine.TimeScale = 1.0f;
		_panelContainer.Visible = false;
		Visible = false;
	}

	public void Pause()
	{
		Engine.TimeScale = 0.0f;
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

	public void OnReferencePressed()
	{
		_panelContainer.Visible = true;
	}

	public void OnCloseButtonPressed()
	{
		_panelContainer.Visible = false;
	}
}
