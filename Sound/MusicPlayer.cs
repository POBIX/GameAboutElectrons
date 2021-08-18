using Godot;
using System;

public class MusicPlayer : AudioStreamPlayer
{
    public static MusicPlayer Ref { get; private set; }

    public MusicPlayer() => Ref = this;
}
