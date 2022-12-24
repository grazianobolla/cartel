using Godot;
using System;

public abstract class TileHandler
{
    public String Label { get; private set; } = "No Label";
    public Tile Parent = null;

    public TileHandler(Tile parent, String label)
    {
        this.Label = label;
        this.Parent = parent;
    }

    public abstract void Init();
}