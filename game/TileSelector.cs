using Godot;
using System;

public class TileSelector : Node
{
    [Export] private NodePath _cameraPath;

    private CameraController _camera;
    public int CurrentIndex = 0;
    public bool Enabled = false;

    public override void _Ready()
    {
        _camera = (CameraController)GetNode(_cameraPath);
    }

    public void Enable()
    {
        _camera.Overview();
        Board.GetTile(CurrentIndex).Highlight(true);
        Enabled = true;
    }

    public void Disable()
    {
        Board.GetTile(CurrentIndex).Highlight(false);
        CurrentIndex = 0;
        Enabled = false;
    }

    public void Move(bool forward)
    {
        Board.GetTile(CurrentIndex).Highlight(false);
        CurrentIndex = Board.GetShiftedIndex(CurrentIndex, forward ? 1 : -1);
        GD.Print(CurrentIndex);
        Board.GetTile(CurrentIndex).Highlight(true);
    }
}
