using Godot;
using System;

public partial class DogDeadForQuest : NpcObject
{
    [Export] private AudioStream _wolfHowling;
    [Export] private Sprite2D aliveSprite;
    [Export] private Sprite2D deadSprite;
    private AudioStreamPlayer2D _howlPlayer;

    private Health _npcHealth;

    private double _howlTimer = 0.0;
    private double _howlInterval = 0.0;
    private Random _rand = new Random();

    public override void _Ready()
    {
        _npcHealth = GetNodeOrNull<Health>("Health");
        _npcHealth?.Set("maxHealth", 10000);
        _npcHealth?.Set("health", 10000);

        aliveSprite.Visible = true;
        deadSprite.Visible = false;

        _howlPlayer = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        // Добавляем AudioStreamPlayer2D
        _howlPlayer.Stream = _wolfHowling;

        SetNewHowlInterval();
    }

    public override void _Process(double delta)
    {
        if (_npcHealth == null)
            return;

        if (_npcHealth.health > 9950)
        {
            _howlTimer += delta;
            if (_howlTimer >= _howlInterval)
            {
                _howlPlayer.Play();
                _howlTimer = 0.0;
                SetNewHowlInterval();
            }
        }
        else
        {
            aliveSprite.Visible = false;
            deadSprite.Visible = true;
            _npcHealth.health = 9950;
        }
    }

    private void SetNewHowlInterval()
    {
        _howlInterval = _rand.NextDouble() * 2.0 + 8.0;
    }
}
