using Godot;
using System;

public static class Spawner
{
    public static T Node<T>(NodePath path, Node parent) where T : Node
    {
        var scene = ResourceLoader.Load<PackedScene>(path);
        var node = scene.Instance() as T;
        parent.CallDeferred("add_child", node);
        return node;
    }

    public static Node Node(NodePath path, Node parent) => Node<Node>(path, parent);

    public static T Node2D<T>(NodePath path, Node parent, Vector2 position) where T : Node2D
    {
        var scene = ResourceLoader.Load<PackedScene>(path);
        var node = scene.Instance() as T;
        node!.GlobalPosition = position;
        parent.CallDeferred("add_child", node);
        return node;
    }

    public static Node2D Node2D(NodePath path, Node parent, Vector2 position) => Node2D<Node2D>(path, parent, position);

    public static ScreenFade Fade(Node parent, Fade fade, Action end)
    {
        var f = Node<ScreenFade>("res://UI/ScreenFade.tscn", parent);
        f.fade = fade;
        f.End += end;
        return f;
    }
}
