using Godot;
using System;

public class Tile : Spatial
{
    public enum Type { NONE, CORNER, PROPERTY, STATE, CHANCE };

    public Type TileType { get; set; } = Type.NONE;
    public int Index { get; private set; } = 0;
    public TileData Data { get; private set; } = null;

    private const int MAX_HOUSE_COUNT = 4;

    private Player _owner = null;
    private Vector3 _defaultPosition;
    private AnimationPlayer _animationPlayer;
    private Tween _tween;

    public override void _Ready()
    {
        _defaultPosition = this.Transform.origin;
        _animationPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
        _tween = (Tween)GetNode("Tween");
    }

    public void Initialize(TileData data, int index)
    {
        Data = data;
        Index = index;
        UpdateVisual();
    }

    public Player PlayerOwner
    {
        set
        {
            _owner = value;
            if (value == null) { SetOwnerIndicator(false, Colors.White); }
            else { SetOwnerIndicator(true, value.Color); }
        }

        get { return _owner; }
    }

    public bool IsOwner(Player player)
    {
        if (PlayerOwner == null)
            return false;

        return player.Id == PlayerOwner.Id;
    }

    public bool IsBuyable()
    {
        return TileType == Type.PROPERTY || TileType == Type.STATE;
    }

    public bool AddHouse()
    {
        if (TileType != Type.PROPERTY)
            return false;

        if (Data.HouseCount >= MAX_HOUSE_COUNT)
            return false;

        Data.HouseCount += 1;

        //TODO: properly spawn model
        Spatial houseModel = Utils.SpawnModel(this, "res://resources/models/defaultHouseModel.tscn");
        houseModel.Translate(new Vector3((float)GD.RandRange(1, -1), 0.25f, (float)GD.RandRange(1, -1)));
        houseModel.RotateY(Mathf.Deg2Rad((float)GD.RandRange(0, 360)));
        //-------------------------
        UpdateVisual();

        return true;
    }

    public void SetOwnerIndicator(bool show, Color color)
    {
        Sprite3D label = GetNode<Sprite3D>("Sprite3D");
        label.Modulate = color;

        if (show)
        {
            _animationPlayer.Play("OwnerIndicatorShow");
            return;
        }

        _animationPlayer.PlayBackwards("OwnerIndicatorShow");
    }

    private void SetGroupMesh(Color color)
    {
        GetNode<MeshInstance>("GroupMesh").Visible = true;
        GetNode<MeshInstance>("GroupMesh").GetSurfaceMaterial(0).Set("albedo_color", color);
    }

    private void SetText(String text)
    {
        GetNode<Label>("Viewport/Label").Text = text;
    }

    private void UpdateVisual()
    {
        switch (this.TileType)
        {
            case Type.PROPERTY:
            case Type.STATE:
                SetGroupMesh(Data.Color);
                SetText(Data.Label);
                break;

            case Type.CORNER:
            case Type.CHANCE:
            case Type.NONE:
            default:
                break;
        }
    }

    public void Highlight(bool enabled, float animationTime = 0.5f, float heightOffset = 2.0f)
    {
        GetNode<MeshInstance>("Mesh").GetSurfaceMaterial(0).NextPass.Set("shader_param/enabled", enabled);
        Vector3 target = _defaultPosition + (new Vector3(0, heightOffset, 0) * (enabled ? 1 : 0));
        _tween.InterpolateProperty(this, "translation", null, target, animationTime, Tween.TransitionType.Cubic);
        _tween.Start();
    }

    //TODO: debug
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