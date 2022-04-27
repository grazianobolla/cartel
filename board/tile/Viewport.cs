using Godot;
using System;

public class Viewport : Godot.Viewport
{
    public override void _Ready()
    {
        UpdateViewport();
    }

    public void UpdateViewport()
    {
        this.Size = GetNode<Label>("Label").RectSize;
    }
}