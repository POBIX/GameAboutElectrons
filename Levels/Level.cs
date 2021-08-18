using Godot;
using System;

public class Level : Node2D
{
    [Export] public readonly bool zap = true;
    [Export] public readonly int deathHeight = 500;

    public static Level Active { get; private set; }

    public static int CurrentLevel { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Active = this;
    }

    public static void ResetCounter() => CurrentLevel = 0;

    public static void Next() => Active.GetTree().ChangeScene($"res://Levels/Level{++CurrentLevel}.tscn");
}
