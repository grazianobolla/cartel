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

    public void Enable(int index)
    {
        CurrentIndex = index;

        _camera.Overview();
        Board.GetTile(CurrentIndex).Highlight(true);
        Enabled = true;
    }

    public void Disable()
    {
        Board.GetTile(CurrentIndex).Highlight(false);
        Enabled = false;
    }

    public void Move(bool forward)
    {
        Board.GetTile(CurrentIndex).Highlight(false);
        CurrentIndex = Board.GetShiftedIndex(CurrentIndex, forward ? 1 : -1);
        Board.GetTile(CurrentIndex).Highlight(true);
    }

    public void MoveTo(int index)
    {
        Board.GetTile(CurrentIndex).Highlight(false);
        CurrentIndex = index;
        Board.GetTile(CurrentIndex).Highlight(true);
    }
}
