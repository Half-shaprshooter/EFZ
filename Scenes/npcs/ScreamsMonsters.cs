using Godot;
using System;

public partial class ScreamsMonsters : Node2D
{
    [Export] private AudioStream[] MonsterScreams; // Массив звуков криков

    private AudioStreamPlayer2D _player;
    private Random _rand = new Random();
    private double _timer = 0.0;
    private double _nextScreamIn = 0.0;

    public override void _Ready()
    {
        _player = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        SetNextScreamDelay();
    }

    public override void _Process(double delta)
    {
        if (MonsterScreams.Length == 0)
            return;

        _timer += delta;

        if (_timer >= _nextScreamIn)
        {
            PlayRandomScream();
            _timer = 0.0;
            SetNextScreamDelay();
        }
    }

    private void SetNextScreamDelay()
    {
        // 7 to 15 seconds
        _nextScreamIn = _rand.NextDouble() * 8.0 + 7.0;
    }

    private void PlayRandomScream()
    {
        int index = _rand.Next(MonsterScreams.Length);
        _player.Stream = MonsterScreams[index];
        _player.Play();
    }
}

