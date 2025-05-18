using Godot;
using System;
using Godot.Collections;

public partial class Note : Node2D
{
    
    private Control _interactUI;
    [Export] private string _noteText;
    private bool _playerInArea = false;
    private CanvasLayer _noteUI;
    
    public override void _Ready()
    {
        _interactUI = GetNode<Control>("interactItem");
        _noteUI = GetNode<CanvasLayer>("NoteUI");
        _interactUI.Visible = false;
        _noteUI.Visible = false;
        _noteUI.GetNode<Label>("Label").Text = _noteText;
    }

    public override void _Process(double delta)
    {
        if (_playerInArea && Input.IsActionJustPressed("interact") && !_noteUI.Visible)
        {
            _noteUI.Visible = true;
            GetTree().Paused = true;
        }

        if (_noteUI.Visible && GetTree().Paused && Input.IsActionJustPressed("pause"))
        {
            CloseNote();
        }
    }

    private void CloseNote()
    {
        _noteUI.Visible = false;
        GetTree().Paused = false;
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

    /// <summary>
    ///  Обработка выхода игрока из зону предмета
    /// </summary>
    /// <param name="body">тело вхожденного объекта</param>
    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            _interactUI.Visible = false;
            _playerInArea = false;
        }
    }
}
