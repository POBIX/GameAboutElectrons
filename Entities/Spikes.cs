using Godot;
using System;

public class Spikes : Area2D
{
    // ReSharper disable once UnusedMember.Local (signal)
    private void OnHit(Player p) => p.Die();
}
