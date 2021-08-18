using Godot;
using System;

public class Skipper : CanvasLayer
{
    [Export] private PackedScene scene;

    public override void _Ready()
    {
        base._Ready();

        MusicPlayer.Ref.Stop();
    }

    private void Timeout() => GetTree().ChangeSceneTo(scene);

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (Input.IsActionJustPressed("Skip"))
            Timeout();
    }
}
