using Godot;
using System;

///<summary>
///Similar to Tween, but it interpolates Transforms properly,
///for some reason Tweens have problems with that.
///</summary>
public class TransformInterpolator : Tween
{
    [Signal] public delegate void FinishedInterpolation();

    private Transform _to;
    private Spatial _spatial;

    public override void _Ready()
    {
        Connect("tween_completed", this, "OnTweenCompleted");
    }

    public void InterpolateTransform(Spatial spatial, Transform to)
    {
        _to = to;
        _spatial = spatial;
    }

    public void Start(float duration, Tween.TransitionType transType = Tween.TransitionType.Cubic)
    {
        InterpolateMethod(this, "Interpolate", 0, 1, duration, transType);
        Start();
    }

    public void OnTweenCompleted(Godot.Object obj, NodePath key)
    {
        EmitSignal(nameof(FinishedInterpolation));
    }

    private void Interpolate(float weight)
    {
        _spatial.Transform = _spatial.Transform.InterpolateWith(_to, weight);
    }
}
