using Godot;
using System;

public class Level0 : Level
{
    public override void _Ready()
    {
        base._Ready();
        SpeedrunTimer.Ref.SetProcess(true);
        MusicPlayer.Ref.Play();
    }
}
