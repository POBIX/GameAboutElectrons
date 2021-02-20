// #define HTML5

using Godot;
using System;

public class MainMenu : CanvasLayer
{
    public override void _Ready()
    {
        base._Ready();

        GetNode<Button>("Background/Button").GrabFocus();
#if HTML5
        GetNode("Background/Button2").QueueFree();
#endif
    }

    // ReSharper disable once UnusedMember.Local (signal)
    private void Start() => GetTree().ChangeScene("res://Levels/Level0.tscn");

#if !HTML5
    public override void _Process(float delta)
    {
        base._Process(delta);

        if (Input.IsActionJustPressed("Quit"))
            Quit();
    }

    private void Quit() => GetTree().Quit();
#endif

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        OS.WindowFullscreen = true;
    }
}
