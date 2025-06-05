using Godot;
using System;
using System.Data.Common;

public partial class CodeLock : Node
{
    //спрайты вариаций кодового замка
    private Sprite2D _zeroButtons;
    private Sprite2D _oneButtons;
    private Sprite2D _twoButtons;
    private Sprite2D _threeButtons;
    private Sprite2D _fourButtons;
    private Sprite2D _error;

    //canvas всего что относится к замку
    private CanvasLayer _codeUICanvas;

    //начался ли процесс ввода кода
    public bool starting = false;

    //переменная отвечающая за то, закрыт ли объект на замок
    [Export] public bool isLocked;

    //нужный пинкод для открытия
    [Export] public string PinCode;

    //вводимый нами код
    private string inputCode = "";

    public override void _Ready()
	{
        _zeroButtons = GetNode<Sprite2D>("CodeUI/CodePictures/ZeroButtons");
        _oneButtons = GetNode<Sprite2D>("CodeUI/CodePictures/OneButton");
        _twoButtons = GetNode<Sprite2D>("CodeUI/CodePictures/TwoButtons");
        _threeButtons = GetNode<Sprite2D>("CodeUI/CodePictures/ThreeButtons");
        _fourButtons = GetNode<Sprite2D>("CodeUI/CodePictures/FourButtons");
        _error = GetNode<Sprite2D>("CodeUI/CodePictures/Error");

        _codeUICanvas = GetNode<CanvasLayer>("CodeUI");

        _codeUICanvas.Visible = false;
        _zeroButtons.Visible = false;
	}

    public override void _Process(double delta)
	{
        //если нажать кнопку и при этом процесс ввода уже начался, то мы его отменим
        if (Input.IsActionJustPressed("Cancel_uncoding") && starting)
        {
            CancelPinCode();
        }
	}


    //метод для появления кодового замка на экране и начала работы с ним
    public void StartPinCode()
    {
        starting = true;
        GetTree().Paused = true; 
        inputCode = "";
        _codeUICanvas.Visible = true;
        _zeroButtons.Visible = true;
        _oneButtons.Visible = false;
        _threeButtons.Visible = false;
        _fourButtons.Visible = false;
        _error.Visible = false;
    }

    //метод отмены ввода пинкода
    public void CancelPinCode()
    {
        GetTree().Paused = false; 
        starting = false;
        _codeUICanvas.Visible = false;
        CodeDelete();
    }


    //метод для добавления новой цифры к коду
    private void CodeAdder(string number)
    {
        if (inputCode.Length <= 4 && inputCode.Length >= 0)
        {
            inputCode += number;

            switch (inputCode.Length)
            {
                case 1:
                _oneButtons.Visible = true;   
                    break;

                case 2:
                _twoButtons.Visible = true;    
                    break;

                case 3:
                _threeButtons.Visible = true;    
                    break;

                case 4:
                _fourButtons.Visible = true;    
                    break;
            }

        }

        else
        {
            
        }
    }

    //метод для сравнения кодов
    private void CodeCompare()
    {
        if (PinCode == inputCode)
        {
            starting = false;
            isLocked = false;
            _codeUICanvas.Visible = false;
            CodeDelete();
            GetTree().Paused = false; 
            GD.Print("Закрыто ли?" + isLocked);
        }
        else
        {
            _error.Visible = true;
        }
    }

    //метод для обнуления кода
    private void CodeDelete()
    {
        inputCode = "";
        _zeroButtons.Visible = true;
        _oneButtons.Visible = false;
        _twoButtons.Visible = false;
        _threeButtons.Visible = false;
        _fourButtons.Visible = false;
        _error.Visible = false;
    }

    //методы которые срабатывают при нажатии определённой кнопки
    private void _on_one_pressed()
    {
        CodeAdder("1");
    }

    private void _on_two_pressed()
    {
        CodeAdder("2");
    }

    private void _on_three_pressed()
    {
        CodeAdder("3");
    }

    private void _on_four_pressed()
    {
        CodeAdder("4");
    }

    private void _on_five_pressed()
    {
        CodeAdder("5");
    }

    private void _on_six_pressed()
    {
        CodeAdder("6");
    }

    private void _on_seven_pressed()
    {
        CodeAdder("7");
    }

    private void _on_eight_pressed()
    {
        CodeAdder("8");
    }

    private void _on_nine_pressed()
    {
        CodeAdder("9");
    }

    private void _on_zero_pressed()
    {
        CodeAdder("0");
    }

    private void _on_cancel_pressed()
    {
        CodeDelete();
    }

    private void _on_enter_pressed()
    {
        CodeCompare();
    }
    
}
