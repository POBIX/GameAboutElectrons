using Godot;
using System;

public static class Sound
{
    public static AudioStreamPlayer Create(string path, Node parent, int volume = 0)
    {
        var player = new AudioStreamPlayer
        {
            Bus = "SoundEffects",
            Stream = ResourceLoader.Load<AudioStream>($"res://Sound/{path}"),
            VolumeDb = volume
        };
        parent.AddChild(player);
        player.Play();
        return player;
    }

    public static async void Play(string path, Node parent, int volume = 0)
    {
        AudioStreamPlayer p = Create(path, parent, volume);
        await p.ToSignal(p, "finished");
        p.QueueFree();
    }
}
