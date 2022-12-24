using Godot;
using System;

class TradeableHandler : TileHandler
{
    public int Price { get; private set; } = 0;
    public int Group { get; private set; } = -1;
    public Color Color { get; private set; } = Colors.White;

    public int HouseCount { get; private set; } = 0;
    public int HousePrice { get; private set; } = 10;
    public int LandingFee { get; private set; } = 10;

    private Player _owner = null;
    private AnimationPlayer _animationPlayer;

    public TradeableHandler(Tile parent, string label, int price, int group, Color color) : base(parent, label)
    {
        _animationPlayer = (AnimationPlayer)parent.GetNode("AnimationPlayer");
        this.Price = price;
        this.Group = group;
        this.Color = color;
    }

    public override void Init()
    {
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
        {
            return false;
        }

        return player.Id == PlayerOwner.Id;
    }

    public bool AddHouse()
    {
        if (this.Parent.TileType == Tile.Type.STATE)
        {
            GD.PrintErr("trying to add house to state tile");
            return false;
        }

        if (HouseCount >= 4) //TODO: magic num
        {
            return false;
        }

        HouseCount += 1;

        //TODO: properly spawn model
        Spatial houseModel = Utils.SpawnModel(this.Parent, "res://resources/models/defaultHouseModel.tscn");
        houseModel.Translate(new Vector3((float)GD.RandRange(1, -1), 0.25f, (float)GD.RandRange(1, -1)));
        houseModel.RotateY(Mathf.Deg2Rad((float)GD.RandRange(0, 360)));
        //-------------------------
        UpdateVisual();

        return true;
    }

    public void SetOwnerIndicator(bool show, Color color)
    {
        Sprite3D label = this.Parent.GetNode<Sprite3D>("Sprite3D");
        label.Modulate = color;

        if (show)
        {
            _animationPlayer.Play("OwnerIndicatorShow");
            return;
        }

        _animationPlayer.PlayBackwards("OwnerIndicatorShow");
    }

    private void UpdateVisual()
    {
        SetGroupMesh(Color);
        SetText(Label);
    }

    private void SetGroupMesh(Color color)
    {
        this.Parent.GetNode<MeshInstance>("GroupMesh").Visible = true;
        this.Parent.GetNode<MeshInstance>("GroupMesh").GetSurfaceMaterial(0).Set("albedo_color", color);
    }

    private void SetText(String text)
    {
        var label = this.Parent.GetNode<Label3D>("Label3D");
        const int MAX_LABEL_LENGHT_COUNT = 14; //TODO: magic number

        label.Visible = true;

        if (text.Length >= MAX_LABEL_LENGHT_COUNT)
        {
            text = $"{text.Substr(0, MAX_LABEL_LENGHT_COUNT).StripEdges()}.";
        }

        label.Text = text;
    }
}