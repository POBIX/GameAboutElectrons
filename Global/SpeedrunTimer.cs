using Godot;
using System;

public class SpeedrunTimer : Node
{
    public static float Time { get; private set; }

    public override void _Process(float delta)
    {
        base._Process(delta);

        Time += delta;
    }
}
