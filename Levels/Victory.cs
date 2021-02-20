// #define HTML5

using Godot;
using System;

public class Victory : CanvasLayer
{
    public override void _Ready()
    {
        base._Ready();

        GetNode<Label>("Time").Text += $"{Math.Round(SpeedrunTimer.Time, 1)} seconds!";
#if HTML5
        GetNode("Button").QueueFree();
#endif
    }

#if !HTML5
    // ReSharper disable once UnusedMember.Local (signal)
    private void Quit() => GetTree().ChangeScene("res://UI/MainMenu.tscn");
#endif
}
