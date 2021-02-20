using Godot;
using System;

public class Background : Sprite
{
    private Random rnd = new Random();
    private Timer timer;
    private AnimatedTexture tex;
    [Export] private int minTime = 5, maxTime = 20;

    private Vector2 velocity = new Vector2(10, 0);

    public override void _Ready()
    {
        base._Ready();
        timer = GetNode<Timer>("../Timer");
        tex = (AnimatedTexture)Texture;
        tex.Frame0__delaySec = minTime;
        tex.Pause = false;

        Spawner.Fade(GetParent().GetParent(), Fade.Out, null);
        GetNode<ColorRect>("../../../ColorRect").QueueFree();
    }

    // ReSharper disable once UnusedMember.Local (signal)
    private void Timeout()
    {
        timer.WaitTime = rnd.Next(minTime, maxTime);
        tex.Frame0__delaySec = timer.WaitTime;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        Position += velocity * delta;
    }
}
