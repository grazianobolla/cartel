using Godot;
using System;

public class CameraController : Camera
{
    [Export] private Vector3 _overviewTarget = new Vector3(0, 55, 0);
    [Export] private float _overviewFOV = 35;
    [Export] private Vector3 _focusOffset = new Vector3(-10, 10, 0);
    [Export] private float _focusFOV = 50;

    public enum State { OVERVIEW, FOCUSING };
    public State currentState { get; private set; } = State.OVERVIEW;

    private Tween _tween;
    private Transform _overviewTransform;
    private Spatial _playerTarget;
    private float _weight;

    public override void _Ready()
    {
        _tween = (Tween)GetNode("Tween");

        _overviewTransform = GetOverviewTransform();
        Overview();
    }

    public override void _PhysicsProcess(float delta)
    {
        float weight = delta * _weight;

        switch (currentState)
        {
            case State.FOCUSING:
                {
                    Vector3 targetPos = _playerTarget.GlobalTransform.origin;
                    Transform targetTransform = this.GlobalTransform;
                    targetTransform.origin = targetPos + _focusOffset;
                    targetTransform = targetTransform.LookingAt(targetPos, Vector3.Up);

                    this.Transform = this.Transform.InterpolateWith(targetTransform, weight);
                    this.Fov = this.Fov + (_focusFOV - this.Fov) * weight;
                    break;
                }

            case State.OVERVIEW:
                {
                    this.Transform = this.Transform.InterpolateWith(_overviewTransform, weight);
                    this.Fov = this.Fov + (_overviewFOV - this.Fov) * weight;
                    break;
                }
        }

    }

    public async void Overview(float weight = 3, float delay = 0)
    {
        await ToSignal(GetTree().CreateTimer(delay), "timeout");

        _weight = weight;
        currentState = State.OVERVIEW;
    }

    public async void FocusPlayer(Spatial player, float weight = 3, float delay = 0)
    {
        await ToSignal(GetTree().CreateTimer(delay), "timeout");

        _playerTarget = player;
        _weight = weight;
        currentState = State.FOCUSING;
    }

    private Transform GetOverviewTransform()
    {
        Transform t = Transform.Identity;
        t.origin = _overviewTarget;
        t.basis = Basis.Identity;
        t.basis = t.basis.Rotated(Vector3.Right, -Mathf.Pi / 2);
        t.basis = t.basis.Rotated(Vector3.Up, -Mathf.Pi / 2);
        return t;
    }
}
