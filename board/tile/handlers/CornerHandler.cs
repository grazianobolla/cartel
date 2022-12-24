using Godot;
using System;

class CornerHandler : TileHandler
{
    public int CornerNumber { get; private set; } = -1;

    public override void Init() { }

    public CornerHandler(Tile parent, string label, int number) : base(parent, label)
    {
        this.CornerNumber = number;
    }
}