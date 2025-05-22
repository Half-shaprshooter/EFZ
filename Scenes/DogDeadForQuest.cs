using Godot;
using System;

public partial class DogDeadForQuest : NpcObject
{
    private Health _npcHealth;

    [Export] Sprite2D aliveSprite;
    [Export] Sprite2D deadSprite;

    public override void _Ready()
    {
        _npcHealth = GetNodeOrNull<Health>("Health");
        _npcHealth.Set("maxHealth", 10000);
        _npcHealth.Set("health", 10000);
        GD.Print(_npcHealth.health);

        aliveSprite.Visible = true;
        deadSprite.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_npcHealth.health <= 9950)
        {
            aliveSprite.Visible = false;
            deadSprite.Visible = true;
            _npcHealth.health = 9950;
        }
    }
}
