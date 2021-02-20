using Godot;
using System;

public enum Fade
{
    In,
    Out
};

public class ScreenFade : CanvasLayer
{
    public event Action End;
    public Fade fade;

    public override void _Ready()
    {
        base._Ready();

        GetNode<AnimationPlayer>("AnimationPlayer").Play($"Fade{fade}");
    }

    // ReSharper disable once UnusedMember.Local (signal)
    // ReSharper disable once UnusedParameter.Local
    private void Finished(string name)
    {
        End?.Invoke();
        QueueFree();
    }
}
