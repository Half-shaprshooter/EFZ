using Godot;
using System;

public partial class OneTimeHelp : Node2D
{
    //ссылка на label подсказки
    [Export] private Label hint;

    //выбор нужного действия
    [Export] private string action;

    //если игрок рядом = true
    private bool isPlayerNear = false;

    //было ли сделано действие, нужное для подсказки
    private bool isActivated = false;

    //текст подсказки
    [Export] private string hintText = "";

    public override void _Ready()
    {
        hint.Text = hintText;
        hint.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (!isActivated && isPlayerNear)
        {
            hint.Visible = true;

            if (Input.IsActionJustPressed(action))
            {
                GD.Print("Пизда");
                hint.Visible = false;
                isActivated = true;
            }
        }
        else
        {
            hint.Visible = false;
        }
    }

    private void PlayerNearEntered(Node2D body)
    {
        if (body is PlayerControl)
        {
            isPlayerNear = true;
        }
    }

    private void PlayerNearExited(Node2D body)
    {
        if (body is PlayerControl)
        {
            isPlayerNear = false;
        }
    }
}