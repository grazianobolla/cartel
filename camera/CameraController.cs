using Godot;
using System.Threading.Tasks;

public class CameraController : Camera
{
    [Export] private Vector3 _overviewTarget = new Vector3(0, 60, 0);
    [Export] private float _overviewFOV = 35;
    [Export] private Vector3 _focusOffset = new Vector3(-10, 10, 0);
    [Export] private float _focusFOV = 50;


    public enum State { OVERVIEW, FOCUS };
    public State CurrentState { get; private set; } = State.OVERVIEW;

    private Transform _overviewTransform;
    private Spatial _target;
    private float _weight;

    private const float MIN_DISTANCE = 0.5f;

    public override void _Ready()
    {
        _overviewTransform = GetOverviewTransform();
    }

    public override void _PhysicsProcess(float delta)
    {
        float weight = delta * _weight;

        switch (currentState)
        {
            case State.FOCUS:
                {
                    Vector3 targetPos = _target.GlobalTransform.origin;
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

    public void Overview(float weight = 3)
    {
        _weight = weight;
        currentState = State.OVERVIEW;
    }

    public void Focus(Spatial player, float weight = 3)
    {
        _target = player;
        _weight = weight;
        currentState = State.FOCUS;
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
