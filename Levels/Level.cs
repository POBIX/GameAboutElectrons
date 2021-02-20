using Godot;
using System;

public class Level : Node2D
{
    [Export] public readonly bool zap = true;
    [Export] public readonly int deathHeight = 500;
}
