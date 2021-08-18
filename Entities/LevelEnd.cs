using Godot;
using System;

public class LevelEnd : Area2D
{
    // ReSharper disable once UnusedMember.Local (signal)
    private void Entered(Player p) => Level.Next();
}
