using Godot;
using System;

public class NameTag : Control
{
    private bool _enabled = false;

    private Camera _camera;
    private Position3D _parent;
    private Label _label;

    public override void _Ready()
    {
        _camera = GetViewport().GetCamera();
        _parent = (Position3D)GetParent();
        _label = (Label)GetNode("NameLabel");
    }

    public override void _Process(float delta)
    {
        if (!_enabled)
            return;


        Vector3 parentTranslation = _parent.GlobalTransform.origin;

        bool isBehind = _camera.GlobalTransform.basis.z.Dot(parentTranslation - _camera.GlobalTransform.origin) > 0;
        this.Visible = !isBehind;

        Vector2 unprojectedPos = _camera.UnprojectPosition(parentTranslation);
        _label.RectPosition = unprojectedPos + new Vector2(_label.RectSize.x / -2, 0);
    }

    public void SetTagProperties(string tag, Color color)
    {
        _label.Text = tag;
        _label.Modulate = color;
    }

    public void ToggleEnable(bool value)
    {
        _enabled = value;
        this.Visible = value;
    }
}
