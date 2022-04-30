using Godot;
using System;

public class CameraController : Camera
{
    [Export] private Vector3 _overviewTarget = new Vector3(0, 55, 0);
    [Export] private float _overviewFOV = 35;
    [Export] private float _focusFOV = 70;
    [Export] private Vector3 _focusDistance = new Vector3(0, 5, 5);

    private Tween _tween;

    public override void _Ready()
    {
        _tween = (Tween)GetNode("Tween");
    }

    //TODO: check this
    public void Overview(float timeSec)
    {
        Transform targetTransform = this.GlobalTransform;
        targetTransform.origin = _overviewTarget;
        targetTransform.basis = Basis.Identity.Rotated(Vector3.Right, -Mathf.Pi / 2);

        _tween.InterpolateProperty(this, "transform", null, targetTransform, timeSec, Tween.TransitionType.Cubic);
        _tween.InterpolateProperty(this, "fov", null, _overviewFOV, timeSec, Tween.TransitionType.Cubic);
        _tween.Start();
    }

    //TODO: check this
    public void FocusPlayer(Spatial player, float timeSec)
    {
        Transform targetTransform = this.GlobalTransform;
        targetTransform.origin = player.GlobalTransform.origin + _focusDistance.Rotated(Vector3.Up, player.Rotation.y);
        targetTransform.basis = Basis.Identity.Rotated(Vector3.Right, -Mathf.Pi / 4);
        targetTransform.basis = targetTransform.basis.Rotated(Vector3.Up, player.Rotation.y);

        _tween.InterpolateProperty(this, "transform", null, targetTransform, timeSec, Tween.TransitionType.Cubic);
        _tween.InterpolateProperty(this, "fov", null, _focusFOV, timeSec, Tween.TransitionType.Cubic);
        _tween.Start();
    }
}
