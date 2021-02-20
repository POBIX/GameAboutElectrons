using Godot;
using System;

public class EntityMap : TileMap
{
    public override void _Ready()
    {
        base._Ready();

        foreach (Vector2 pos in GetUsedCells())
        {
            string nameW = TileSet.TileGetName(GetCellv(pos));
            if (nameW[0] == '@') continue;
            int i = nameW.IndexOf(':');
            string name = i != -1 ? nameW.Remove(i) : nameW;
            SetCellv(pos, -1);
            Vector2 p = MapToWorld(pos) + new Vector2(8, 8); // Tiles spawn with an offset
            Node2D n = Spawner.Node2D($"res://Entities/{name}.tscn", this, p);
            if (i != -1) n.RotationDegrees = int.Parse(nameW.Substring(i + 1));
        }
    }
}
