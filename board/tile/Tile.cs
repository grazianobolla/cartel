using Godot;
using System;


public class Tile : Spatial
{
    public enum Type { NONE, CORNER, PROPERTY, STATE, CHANCE };

    public Type TileType { get; set; } = Type.NONE;
    public int Index { get; private set; } = 0;

    public TileHandler Handler;

    private Vector3 _defaultPosition;
    private Tween _tween;

    public override void _Ready()
    {
        _defaultPosition = this.Transform.origin;
        _tween = (Tween)GetNode("Tween");
    }

    public void Initialize(TileHandler handler, int index)
    {
        this.Handler = handler;
        this.Index = index;

        this.Handler.Init();
    }

    public void Highlight(bool enabled, float animationTime = 0.5f, float heightOffset = 2.0f)
    {
        GetNode<MeshInstance>("Mesh").GetSurfaceMaterial(0).NextPass.Set("shader_param/enabled", enabled);
        Vector3 target = _defaultPosition + (new Vector3(0, heightOffset, 0) * (enabled ? 1 : 0));
        _tween.InterpolateProperty(this, "translation", null, target, animationTime, Tween.TransitionType.Cubic);
        _tween.Start();
    }

    public bool IsBuyable()
    {
        return TileType == Type.PROPERTY || TileType == Type.STATE;
    }

    private void _on_DebugArea_input_event(Node camera, InputEvent ev, Vector3 position, Vector3 normal, int shapeIdx)
    {
        if (ev is InputEventMouseButton eventButton)
        {
            if (!eventButton.IsPressed())
                return;

            switch (eventButton.ButtonIndex)
            {
                case (int)ButtonList.Left:
                    GetNode<Controller>("/root/Controller").SendDebugShakeMethod(Index);
                    break;
            }
        }
    }
}