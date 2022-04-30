using Godot;
using System;

public class TransformInterpolator : Node
{
    [Signal] public delegate void FinishedInterpolation();

    private Transform _to;
    private Godot.Object _obj;
    private Tween _tween;

    public override void _Ready()
    {
        _tween = (Tween)GetNode("Tween");
        _tween.Connect("tween_completed", this, "OnTweenCompleted");
    }

    public void InterpolateTransform(Godot.Object obj, Transform to)
    {
        _to = to;
        _obj = obj;
    }

    private void Interpolate(float weight)
    {
        Spatial spatial = (Spatial)_obj;
        spatial.Transform = spatial.Transform.InterpolateWith(_to, weight);
    }

    public void Start(float duration)
    {
        _tween.InterpolateMethod(this, "Interpolate", 0, 1, duration, Tween.TransitionType.Cubic);
        _tween.Start();
    }

    public void OnTweenCompleted(Godot.Object obj, NodePath key)
    {
        EmitSignal(nameof(FinishedInterpolation));
    }
}
