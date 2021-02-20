// #define HTML5

using Godot;
using System;

public class Pause : CanvasLayer
{
    private bool firstFrame = true;

    public static bool annoyingBug = false;

    public override void _Ready()
    {
        base._Ready();

        GetNode<Button>("ColorRect/Button2").GrabFocus();
#if HTML5
        GetNode("ColorRect/Button").QueueFree();
#endif
    }

    // ReSharper disable once UnusedMember.Local (signal)
    private void Quit()
    {
    #if !HTML5
        GetTree().Quit();
    #endif
    }

    // ReSharper disable once UnusedMember.Local (signal)
    private void Resume()
    {
        GetTree().Paused = false;
        QueueFree();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (firstFrame)
        {
            firstFrame = false;
            return;
        }

        if (Input.IsActionJustPressed("Quit"))
        {
            Resume();
            annoyingBug = true;
        }
    }
}
