using Godot;
using System;

public class SpeedrunTimer : Node
{
    public static float Time { get; private set; }
    public static SpeedrunTimer Ref { get; private set; }
    public SpeedrunTimer() => Ref = this;

    public override void _Ready()
    {
        base._Ready();

        SetProcess(false);
    }

    public static void ResetTime() => Time = 0;

    public override void _Process(float delta)
    {
        base._Process(delta);

        Time += delta;
    }
}
