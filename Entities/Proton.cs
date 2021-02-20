using Godot;
using System;

public class Proton : Area2D
{
    private static Random r = new Random();

    public override void _Ready()
    {
        base._Ready();

        var anim = GetNode<AnimationPlayer>("AnimationPlayer");
        var sprite = GetNode<Sprite>("Sprite");
        anim.Advance(r.Next(0, sprite.Hframes));
    }
}
