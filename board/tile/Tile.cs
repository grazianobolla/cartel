using Godot;
using System;

public struct TileData
{
    public String label { get; }
    public int price { get; }
    public int houses { get; set; }
    public int group { get; }
    public Color color { get; }

    public TileData(String label, int price, int group, Color color)
    {
        this.label = label;
        this.price = price;
        this.houses = 0;
        this.group = group;
        this.color = color;
    }
}

public class Tile : Spatial
{
    private const int MAX_HOUSE_COUNT = 4;

    public enum Type { NONE, CORNER, PROPERTY, STATE, CHANCE };
    public Type type { get; set; } = Type.NONE;
    public int index { get; private set; } = 0;
    public Player owner { get; set; } = null;

    private TileData _data;

    public void Initialize(TileData data, int index)
    {
        this._data = data;
        this.index = index;
        UpdateVisual();
    }

    public bool IsOwner(Player player)
    {
        //FIXME: what happens if the player loses connection and reconnects??
        //the saved player will not be the same and this will not work:
        return player == owner;
    }

    public int Group
    {
        get { return _data.group; }
    }

    public int GetFee()
    {
        return _data.houses * 10;
    }

    public bool IsBuyable()
    {
        return type == Type.PROPERTY || type == Type.STATE;
    }

    public int GetPrice()
    {
        return _data.price;
    }

    public String GetLabel()
    {
        return _data.label;
    }

    public int GetHousePrice()
    {
        return 100;
    }

    public bool AddHouse()
    {
        if (type != Type.PROPERTY)
            return false;

        if (_data.houses >= MAX_HOUSE_COUNT)
            return false;

        _data.houses += 1;

        //TODO: properly spawn model
        Spatial houseModel = Utils.SpawnModel(this, "res://resources/models/defaultHouseModel.tscn");
        houseModel.Translate(new Vector3((float)GD.RandRange(0.7, -0.7), 0, -1));
        //-------------------------
        UpdateVisual();

        return true;
    }

    private void UpdateText(String str)
    {

    }

    private void UpdateGroupMesh(Color color)
    {
        GetNode<MeshInstance>("GroupMesh").Visible = true;
        GetNode<MeshInstance>("GroupMesh").GetSurfaceMaterial(0).Set("albedo_color", color);
    }

    private void UpdateVisual()
    {
        switch (this.type)
        {
            case Type.PROPERTY:
                UpdateGroupMesh(_data.color);
                UpdateText($"{_data.label}\n${_data.price}");
                break;

            case Type.STATE:
                UpdateText($"{_data.label}\n${_data.price}");
                break;

            case Type.CORNER:
                UpdateText($"{_data.group}");
                break;

            case Type.CHANCE:
                UpdateText($"{_data.label}");
                break;

            case Type.NONE:
            default:
                UpdateText("Error");
                break;
        }
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
                    GetNode<Controller>("/root/Controller").SendDebugShakeMethod(index);
                    break;

                case (int)ButtonList.Middle:
                    GetNode<TextEdit>("/root/Game/DebugPanel/VBoxContainer/HBoxContainer2/TextEdit").Text = index.ToString();
                    break;
            }
        }
    }
}